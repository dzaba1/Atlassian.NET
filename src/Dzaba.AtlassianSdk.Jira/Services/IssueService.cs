using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

internal sealed class IssueService : IIssueService
{
    // The documented limits of the reference could not be confirmed for these endpoints, so a conservative page size is used.
    // Requesting more than the endpoint allows makes Jira clamp the page silently, which breaks the offset arithmetic.
    private const int MaxCommentsResults = 100;
    private const int MaxWorklogsResults = 100;
    private const int MaxChangeLogResults = 100;
    // Token based paging, so the server is free to return fewer issues than requested.
    private const int MaxSearchResults = 100;
    // Keeps the "key in (...)" clause short enough for the JQL argument limit.
    private const int MaxKeysPerQuery = 100;
    private const int MaxPropertyKeyLength = 255;
    private const string AllFields = "*all";

    // Heavy fields that are not fetched with the issue, they have their own methods.
    private static readonly string[] ExcludedFields = ["comment", "attachment", "issuelinks", "subtasks", "watches", "worklog"];

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IClient _clientV3;
    private readonly ILogger<IssueService> _logger;
    private readonly IIssueLinkService _linkService;
    private readonly IIssueRemoteLinkService _remoteLinkService;
    private readonly IIssueFieldService _fieldService;
    private readonly IAttachmentService _attachmentService;

    public IssueService(IClient clientV3,
        ILogger<IssueService> logger,
        IIssueLinkService linkService,
        IIssueRemoteLinkService remoteLinkService,
        IIssueFieldService fieldService,
        IAttachmentService attachmentService)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(linkService);
        ArgumentNullException.ThrowIfNull(remoteLinkService);
        ArgumentNullException.ThrowIfNull(fieldService);
        ArgumentNullException.ThrowIfNull(attachmentService);

        _clientV3 = clientV3;
        _logger = logger;
        _linkService = linkService;
        _remoteLinkService = remoteLinkService;
        _fieldService = fieldService;
        _attachmentService = attachmentService;
    }

    public async Task<Issue> GetIssueAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting issue {IssueKey}", issueKey);

        var fields = new[] { AllFields }.Concat(ExcludedFields.Select(f => "-" + f)).ToArray();
        var model = await _clientV3.GetIssueAsync(issueKey, fields, null, null, null, null, null, token).ConfigureAwait(false);
        return CreateIssue(model);
    }

    public async Task<IReadOnlyDictionary<string, Issue>> GetIssuesAsync(IEnumerable<string> issueKeys, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(issueKeys);

        var keys = issueKeys.ToArray();
        if (keys.Any(string.IsNullOrEmpty))
        {
            throw new ArgumentException("The issue keys cannot contain null or empty values.", nameof(issueKeys));
        }

        var distinctKeys = keys.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var result = new Dictionary<string, Issue>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < distinctKeys.Length; i += MaxKeysPerQuery)
        {
            var batch = distinctKeys.Skip(i).Take(MaxKeysPerQuery);
            var jql = $"key in ({string.Join(",", batch.Select(QuoteJql))})";

            await foreach (var issue in GetIssuesFromJqlAsync(jql, token).WithCancellation(token).ConfigureAwait(false))
            {
                result[issue.Key] = issue;
            }
        }

        return result;
    }

    public async Task UpdateIssueAsync(Issue issue, IssueUpdateOptions options = null, CancellationToken token = default)
    {
        EnsureCreated(issue);

        _logger.LogInformation("Updating issue {IssueKey}", issue.Key);

        // The model holds all the fields of the issue, but only the editable ones can be sent back.
        var editMeta = await _clientV3.GetEditIssueMetaAsync(issue.Key, null, null, token).ConfigureAwait(false);
        var body = new IssueUpdateDetails
        {
            Fields = FilterFields(issue.Model.Fields, editMeta?.Fields?.Keys)
        };

        var notifyUsers = options != null && options.SuppressEmailNotification ? false : (bool?)null;
        await _clientV3.EditIssueAsync(issue.Key, notifyUsers, null, null, null, null, body, token).ConfigureAwait(false);
    }

    public async Task<string> CreateIssueAsync(Issue issue, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(issue);

        if (issue.Model.Fields == null || issue.Model.Fields.Count == 0)
        {
            throw new ArgumentException("The issue has no fields set.", nameof(issue));
        }

        _logger.LogInformation("Creating issue");

        var body = new IssueUpdateDetails
        {
            Fields = issue.Model.Fields
        };
        var created = await _clientV3.CreateIssueAsync(null, body, token).ConfigureAwait(false);
        return created.Key;
    }

    public async Task DeleteIssueAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Deleting issue {IssueKey}", issueKey);

        await _clientV3.DeleteIssueAsync(issueKey, null, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<Issue> GetIssuesFromJqlAsync(string jql, CancellationToken token = default)
    {
        return GetIssuesFromJqlAsync(new IssueSearchOptions(jql), token);
    }

    public async IAsyncEnumerable<Issue> GetIssuesFromJqlAsync(IssueSearchOptions options, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogInformation("Searching issues with JQL {Jql}", options.Jql);

        var fields = GetSearchFields(options);
        string nextPageToken = null;

        do
        {
            var request = new SearchAndReconcileRequestBean
            {
                Jql = options.Jql,
                Fields = fields,
                MaxResults = MaxSearchResults,
                NextPageToken = nextPageToken
            };

            var page = await _clientV3.SearchAndReconsileIssuesUsingJqlPostAsync(request, token).ConfigureAwait(false);

            if (page.Issues != null)
            {
                foreach (var model in page.Issues)
                {
                    yield return CreateIssue(model);
                }
            }

            nextPageToken = page.IsLast ? null : page.NextPageToken;
        }
        while (!string.IsNullOrEmpty(nextPageToken));
    }

    public async Task ExecuteWorkflowActionAsync(Issue issue, string actionNameOrId, WorkflowTransitionUpdates updates, CancellationToken token = default)
    {
        EnsureCreated(issue);
        ArgumentException.ThrowIfNullOrEmpty(actionNameOrId);

        var isId = int.TryParse(actionNameOrId, NumberStyles.None, CultureInfo.InvariantCulture, out _);

        var transitions = new List<IssueTransition>();
        await foreach (var available in GetActionsAsync(issue.Key, true, token).WithCancellation(token).ConfigureAwait(false))
        {
            transitions.Add(available);
        }

        var transition = isId
            ? transitions.FirstOrDefault(t => t.Id == actionNameOrId)
            : transitions.FirstOrDefault(t => string.Equals(t.Name, actionNameOrId, StringComparison.OrdinalIgnoreCase));

        if (transition == null && !isId)
        {
            throw new InvalidOperationException($"Workflow action with name '{actionNameOrId}' not found.");
        }

        var actionId = transition?.Id ?? actionNameOrId;

        _logger.LogInformation("Executing workflow action {ActionId} on issue {IssueKey}", actionId, issue.Key);

        // Only the fields on the transition screen are accepted by the server.
        var body = new IssueUpdateDetails
        {
            Transition = new IssueTransition { Id = actionId },
            Fields = FilterFields(issue.Model.Fields, transition?.Fields?.Keys)
        };

        if (!string.IsNullOrEmpty(updates?.Comment))
        {
            body.Update = new Dictionary<string, ICollection<FieldUpdateOperation>>
            {
                ["comment"] = new List<FieldUpdateOperation>
                {
                    new FieldUpdateOperation { Add = new Comment { Body = AdfDocument.FromText(updates.Comment) } }
                }
            };
        }

        await _clientV3.DoTransitionAsync(issue.Key, body, token).ConfigureAwait(false);
    }

    public async Task<TimeTrackingDetails> GetTimeTrackingDataAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting time tracking data of issue {IssueKey}", issueKey);

        var model = await _clientV3.GetIssueAsync(issueKey, ["timetracking"], null, null, null, null, null, token).ConfigureAwait(false);
        if (!TryGetField(model, "timetracking", out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return value.Deserialize<TimeTrackingDetails>(JsonOptions);
    }

    public async Task<IReadOnlyDictionary<string, FieldMetadata>> GetFieldsEditMetadataAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting edit metadata of issue {IssueKey}", issueKey);

        var metadata = await _clientV3.GetEditIssueMetaAsync(issueKey, null, null, token).ConfigureAwait(false);
        return metadata?.Fields != null
            ? new Dictionary<string, FieldMetadata>(metadata.Fields)
            : new Dictionary<string, FieldMetadata>();
    }

    public async Task<Comment> AddCommentAsync(string issueKey, Comment comment, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentNullException.ThrowIfNull(comment);

        _logger.LogInformation("Adding a comment to issue {IssueKey}", issueKey);

        return await _clientV3.AddCommentAsync(issueKey, null, comment, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<Comment> GetCommentsAsync(string issueKey, CancellationToken token = default)
    {
        var options = new CommentQueryOptions();
        options.Expand.Add("properties");

        return GetCommentsAsync(issueKey, options, token);
    }

    public IAsyncEnumerable<Comment> GetCommentsAsync(string issueKey, CommentQueryOptions options, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogInformation("Getting comments of issue {IssueKey}", issueKey);

        var expand = options.Expand.Count > 0 ? string.Join(",", options.Expand) : null;

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetCommentsAsync(issueKey, startAt, maxResults, null, expand, t),
            page => page.Comments,
            page => IsLastPage(page.Comments, page.StartAt, page.MaxResults, page.Total),
            MaxCommentsResults,
            token);
    }

    public async Task DeleteCommentAsync(string issueKey, string commentId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(commentId);

        _logger.LogInformation("Deleting comment {CommentId} of issue {IssueKey}", commentId, issueKey);

        await _clientV3.DeleteCommentAsync(issueKey, commentId, token).ConfigureAwait(false);
    }

    public async Task<Comment> UpdateCommentAsync(string issueKey, Comment comment, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentNullException.ThrowIfNull(comment);

        if (string.IsNullOrEmpty(comment.Id))
        {
            throw new ArgumentException("Unable to update the comment due to missing id field.", nameof(comment));
        }

        _logger.LogInformation("Updating comment {CommentId} of issue {IssueKey}", comment.Id, issueKey);

        return await _clientV3.UpdateCommentAsync(issueKey, comment.Id, null, null, null, comment, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<IssueTransition> GetActionsAsync(string issueKey, CancellationToken token = default)
    {
        return GetActionsAsync(issueKey, false, token);
    }

    public async IAsyncEnumerable<IssueTransition> GetActionsAsync(string issueKey, bool expandTransitionFields, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting workflow actions of issue {IssueKey}", issueKey);

        var expand = expandTransitionFields ? "transitions.fields" : null;
        var result = await _clientV3.GetTransitionsAsync(issueKey, expand, null, null, null, null, token).ConfigureAwait(false);
        if (result?.Transitions1 == null)
        {
            yield break;
        }

        foreach (var transition in result.Transitions1)
        {
            yield return transition;
        }
    }

    public async IAsyncEnumerable<Attachment> GetAttachmentsAsync(string issueKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting attachments of issue {IssueKey}", issueKey);

        var model = await _clientV3.GetIssueAsync(issueKey, ["attachment"], null, null, null, null, null, token).ConfigureAwait(false);
        if (!TryGetField(model, "attachment", out var value) || value.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        var attachments = value.Deserialize<List<Model.V3.Attachment>>(JsonOptions);
        foreach (var attachment in attachments)
        {
            yield return new Attachment(attachment, _attachmentService);
        }
    }

    public async IAsyncEnumerable<User> GetWatchersAsync(string issueKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting watchers of issue {IssueKey}", issueKey);

        var watchers = await _clientV3.GetIssueWatchersAsync(issueKey, token).ConfigureAwait(false);
        if (watchers?.Watchers1 == null)
        {
            yield break;
        }

        foreach (var watcher in watchers.Watchers1)
        {
            yield return ToUser(watcher);
        }
    }

    public async Task DeleteWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        _logger.LogInformation("Removing watcher {User} from issue {IssueKey}", usernameOrAccountId, issueKey);

        // Jira Cloud identifies the users by the account id only.
        await _clientV3.RemoveWatcherAsync(issueKey, null, usernameOrAccountId, token).ConfigureAwait(false);
    }

    public async Task AddWatcherAsync(string issueKey, string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        _logger.LogInformation("Adding watcher {User} to issue {IssueKey}", usernameOrAccountId, issueKey);

        await _clientV3.AddWatcherAsync(issueKey, usernameOrAccountId, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<Changelog> GetChangeLogsAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting change logs of issue {IssueKey}", issueKey);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetChangeLogsAsync(issueKey, (int)startAt, maxResults, t),
            page => page.Values,
            page => page.IsLast,
            MaxChangeLogResults,
            token);
    }

    public IAsyncEnumerable<Issue> GetSubTasksAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        return GetIssuesFromJqlAsync($"parent = {QuoteJql(issueKey)}", token);
    }

    public async Task AddAttachmentsAsync(string issueKey, UploadAttachmentInfo[] attachments, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentNullException.ThrowIfNull(attachments);

        if (attachments.Length == 0 || attachments.Any(a => a == null))
        {
            throw new ArgumentException("There has to be at least one attachment and none can be null.", nameof(attachments));
        }

        _logger.LogInformation("Adding {Count} attachment(s) to issue {IssueKey}", attachments.Length, issueKey);

        // The generated operation takes the whole multipart/form-data body as a single stream.
        using var content = new MultipartFormDataContent();
        foreach (var attachment in attachments)
        {
            content.Add(new ByteArrayContent(attachment.Data), "file", attachment.Name);
        }

        using var body = await content.ReadAsStreamAsync().ConfigureAwait(false);
        await _clientV3.AddAttachmentAsync(issueKey, body, token).ConfigureAwait(false);
    }

    public async Task DeleteAttachmentAsync(string issueKey, string attachmentId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(attachmentId);

        _logger.LogInformation("Deleting attachment {AttachmentId} of issue {IssueKey}", attachmentId, issueKey);

        await _clientV3.RemoveAttachmentAsync(attachmentId, token).ConfigureAwait(false);
    }

    public async Task<Worklog> GetWorklogAsync(string issueKey, string worklogId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(worklogId);

        _logger.LogInformation("Getting worklog {WorklogId} of issue {IssueKey}", worklogId, issueKey);

        return await _clientV3.GetWorklogAsync(issueKey, worklogId, null, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<Worklog> GetWorklogsAsync(string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting worklogs of issue {IssueKey}", issueKey);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetIssueWorklogAsync(issueKey, startAt, maxResults, null, null, null, t),
            page => page.Worklogs,
            page => IsLastPage(page.Worklogs, page.StartAt, page.MaxResults, page.Total),
            MaxWorklogsResults,
            token);
    }

    public async Task<Worklog> AddWorklogAsync(string issueKey, Worklog worklog, WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate, string newEstimate = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentNullException.ThrowIfNull(worklog);

        if (worklogStrategy == WorklogStrategy.NewRemainingEstimate)
        {
            ArgumentException.ThrowIfNullOrEmpty(newEstimate);
        }

        _logger.LogInformation("Adding a worklog to issue {IssueKey}", issueKey);

        var adjustEstimate = worklogStrategy switch
        {
            WorklogStrategy.RetainRemainingEstimate => (AdjustEstimate2?)AdjustEstimate2.Leave,
            WorklogStrategy.NewRemainingEstimate => AdjustEstimate2.New,
            _ => null
        };
        var estimate = worklogStrategy == WorklogStrategy.NewRemainingEstimate ? newEstimate : null;

        return await _clientV3.AddWorklogAsync(issueKey, null, adjustEstimate, estimate, null, null, null, worklog, token).ConfigureAwait(false);
    }

    public async Task DeleteWorklogAsync(string issueKey, string worklogId, WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate, string newEstimate = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(worklogId);

        if (worklogStrategy == WorklogStrategy.NewRemainingEstimate)
        {
            ArgumentException.ThrowIfNullOrEmpty(newEstimate);
        }

        _logger.LogInformation("Deleting worklog {WorklogId} of issue {IssueKey}", worklogId, issueKey);

        var adjustEstimate = worklogStrategy switch
        {
            WorklogStrategy.RetainRemainingEstimate => (AdjustEstimate4?)AdjustEstimate4.Leave,
            WorklogStrategy.NewRemainingEstimate => AdjustEstimate4.New,
            _ => null
        };
        var estimate = worklogStrategy == WorklogStrategy.NewRemainingEstimate ? newEstimate : null;

        await _clientV3.DeleteWorklogAsync(issueKey, worklogId, null, adjustEstimate, estimate, null, null, token).ConfigureAwait(false);
    }

    public async Task AssignIssueAsync(string issueKey, string assignee, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(assignee);

        _logger.LogInformation("Assigning issue {IssueKey} to {Assignee}", issueKey, assignee);

        await _clientV3.AssignIssueAsync(issueKey, new User { AccountId = assignee }, token).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<string> GetPropertyKeysAsync(string issueKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting property keys of issue {IssueKey}", issueKey);

        var result = await _clientV3.GetIssuePropertyKeysAsync(issueKey, token).ConfigureAwait(false);
        if (result?.Keys == null)
        {
            yield break;
        }

        foreach (var key in result.Keys)
        {
            yield return key.Key;
        }
    }

    public async Task<IReadOnlyDictionary<string, object>> GetPropertiesAsync(string issueKey, IEnumerable<string> propertyKeys, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentNullException.ThrowIfNull(propertyKeys);

        var result = new Dictionary<string, object>();

        foreach (var propertyKey in propertyKeys.Distinct())
        {
            ValidatePropertyKey(propertyKey);

            _logger.LogInformation("Getting property {PropertyKey} of issue {IssueKey}", propertyKey, issueKey);

            try
            {
                var property = await _clientV3.GetIssuePropertyAsync(issueKey, propertyKey, token).ConfigureAwait(false);
                result[property.Key ?? propertyKey] = property.Value;
            }
            catch (ApiException e) when (e.StatusCode == 404)
            {
                // The requested properties that do not exist are not part of the result.
            }
        }

        return result;
    }

    public async Task SetPropertyAsync(string issueKey, string propertyKey, object obj, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ValidatePropertyKey(propertyKey);
        ArgumentNullException.ThrowIfNull(obj);

        _logger.LogInformation("Setting property {PropertyKey} of issue {IssueKey}", propertyKey, issueKey);

        await _clientV3.SetIssuePropertyAsync(issueKey, propertyKey, obj, token).ConfigureAwait(false);
    }

    public async Task DeletePropertyAsync(string issueKey, string propertyKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ValidatePropertyKey(propertyKey);

        _logger.LogInformation("Deleting property {PropertyKey} of issue {IssueKey}", propertyKey, issueKey);

        try
        {
            await _clientV3.DeleteIssuePropertyAsync(issueKey, propertyKey, token).ConfigureAwait(false);
        }
        catch (ApiException e) when (e.StatusCode == 404)
        {
            // The property that we are trying to delete doesn't exist anyway.
        }
    }

    private Issue CreateIssue(IssueBean model)
    {
        return new Issue(model, this, _linkService, _remoteLinkService, _fieldService);
    }

    private static void EnsureCreated(Issue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        if (string.IsNullOrEmpty(issue.Key))
        {
            throw new ArgumentException("The issue has not been created.", nameof(issue));
        }
    }

    private static void ValidatePropertyKey(string propertyKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyKey);

        if (propertyKey.Length > MaxPropertyKeyLength)
        {
            throw new ArgumentOutOfRangeException(nameof(propertyKey), $"The property key cannot be longer than {MaxPropertyKeyLength} characters.");
        }
    }

    private static List<string> GetSearchFields(IssueSearchOptions options)
    {
        var fields = new List<string>();

        if (options.AdditionalFields == null || options.AdditionalFields.Count == 0)
        {
            fields.Add(AllFields);
            fields.AddRange(ExcludedFields.Select(f => "-" + f));
        }
        else if (options.FetchBasicFields)
        {
            // An excluded field requested explicitly is not excluded anymore.
            fields.Add(AllFields);
            fields.AddRange(ExcludedFields
                .Where(f => !options.AdditionalFields.Contains(f, StringComparer.OrdinalIgnoreCase))
                .Select(f => "-" + f));
        }
        else
        {
            fields.AddRange(options.AdditionalFields.Select(f => f.Trim()));
        }

        return fields;
    }

    private static Dictionary<string, object> FilterFields(IDictionary<string, object> fields, ICollection<string> allowedFields)
    {
        var result = new Dictionary<string, object>();
        if (fields == null || allowedFields == null)
        {
            return result;
        }

        var allowed = new HashSet<string>(allowedFields, StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields)
        {
            if (!allowed.Contains(field.Key))
            {
                continue;
            }

            // Jira does not accept null as a valid value for the components, a cleared field has to be an empty array.
            var isNull = field.Value == null || (field.Value is JsonElement element && element.ValueKind == JsonValueKind.Null);
            result[field.Key] = isNull && string.Equals(field.Key, "components", StringComparison.OrdinalIgnoreCase)
                ? Array.Empty<object>()
                : field.Value;
        }

        return result;
    }

    private static bool TryGetField(IssueBean model, string fieldName, out JsonElement value)
    {
        if (model?.Fields != null && model.Fields.TryGetValue(fieldName, out var raw) && raw is JsonElement element)
        {
            value = element;
            return true;
        }

        value = default;
        return false;
    }

    private static bool IsLastPage<T>(ICollection<T> items, long startAt, long maxResults, long total)
    {
        return items == null || items.Count == 0 || startAt + maxResults >= total;
    }

    private static string QuoteJql(string value)
    {
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    private static User ToUser(UserDetails details)
    {
        return new User
        {
            AccountId = details.AccountId,
            AccountType = Enum.TryParse<UserAccountType>(details.AccountType, true, out var accountType) ? accountType : UserAccountType.Unknown,
            Active = details.Active,
            AvatarUrls = details.AvatarUrls,
            DisplayName = details.DisplayName,
            EmailAddress = details.EmailAddress,
            Key = details.Key,
            Name = details.Name,
            Self = Uri.TryCreate(details.Self, UriKind.Absolute, out var self) ? self : null,
            TimeZone = details.TimeZone
        };
    }
}
