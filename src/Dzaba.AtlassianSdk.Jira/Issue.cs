using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Dzaba.AtlassianSdk.Jira.Services;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A JIRA issue.
/// </summary>
public class Issue : IJiraEntity
{
    private const int MaxPropertyKeyLength = 255;

    private readonly IIssueService _issueService;
    private readonly IIssueLinkService _linkService;
    private readonly IIssueRemoteLinkService _remoteLinkService;

    /// <summary>
    /// Creates an issue wrapping the given model.
    /// </summary>
    /// <param name="model">The issue model. An issue without a key is treated as not created yet.</param>
    /// <param name="issueService">Service used for the operations on the issue.</param>
    /// <param name="linkService">Service used for the issue links.</param>
    /// <param name="remoteLinkService">Service used for the remote links.</param>
    public Issue(IssueBean model,
        IIssueService issueService,
        IIssueLinkService linkService,
        IIssueRemoteLinkService remoteLinkService)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(issueService);
        ArgumentNullException.ThrowIfNull(linkService);
        ArgumentNullException.ThrowIfNull(remoteLinkService);

        Model = model;
        _issueService = issueService;
        _linkService = linkService;
        _remoteLinkService = remoteLinkService;
    }

    /// <summary>
    /// The underlying issue model. Change its fields and call <see cref="SaveChangesAsync"/> to send them to the server.
    /// </summary>
    public IssueBean Model { get; private set; }

    /// <summary>
    /// Id of the issue.
    /// </summary>
    public string Id => Model.Id;

    /// <summary>
    /// Unique key of the issue, null if the issue has not been created yet.
    /// </summary>
    public string Key => Model.Key;

    /// <summary>
    /// Saves field changes to server.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task<Issue> SaveChangesAsync(CancellationToken token = default)
    {
        Issue serverIssue;
        if (string.IsNullOrEmpty(Key))
        {
            var newKey = await _issueService.CreateIssueAsync(this, token).ConfigureAwait(false);
            serverIssue = await _issueService.GetIssueAsync(newKey, token).ConfigureAwait(false);
        }
        else
        {
            await _issueService.UpdateIssueAsync(this, null, token).ConfigureAwait(false);
            serverIssue = await _issueService.GetIssueAsync(Key, token).ConfigureAwait(false);
        }

        Model = serverIssue.Model;
        return serverIssue;
    }

    /// <summary>
    /// Creates a link between this issue and the issue specified.
    /// </summary>
    /// <param name="inwardIssueKey">Key of the issue to link.</param>
    /// <param name="linkName">Name of the issue link type.</param>
    /// <param name="comment">Comment to add to this issue.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task LinkToIssueAsync(string inwardIssueKey, string linkName, string comment = null, CancellationToken token = default)
    {
        EnsureCreated("link issue");

        await _linkService.CreateLinkAsync(Key, inwardIssueKey, linkName, comment, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the issue links associated with this issue.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<IssueLink> GetIssueLinksAsync(CancellationToken token = default)
    {
        EnsureCreated("get issue links");

        return _linkService.GetLinksForIssueAsync(this, null, token);
    }

    /// <summary>
    /// Gets the issue links associated with this issue.
    /// </summary>
    /// <param name="linkTypeNames">Optional subset of link types to retrieve.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<IssueLink> GetIssueLinksAsync(IEnumerable<string> linkTypeNames, CancellationToken token = default)
    {
        EnsureCreated("get issue links");

        return _linkService.GetLinksForIssueAsync(this, linkTypeNames, token);
    }

    /// <summary>
    /// Creates an remote link for an issue.
    /// </summary>
    /// <param name="remoteUrl">Remote url to link to.</param>
    /// <param name="title">Title of the remote link.</param>
    /// <param name="summary">Summary of the remote link.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task AddRemoteLinkAsync(string remoteUrl, string title, string summary = null, CancellationToken token = default)
    {
        EnsureCreated("add remote link");

        await _remoteLinkService.CreateRemoteLinkAsync(Key, remoteUrl, title, summary, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the remote links associated with this issue.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<RemoteIssueLink> GetRemoteLinksAsync(CancellationToken token = default)
    {
        EnsureCreated("get remote links");

        return _remoteLinkService.GetRemoteLinksForIssueAsync(Key, token);
    }

    /// <summary>
    /// Transition an issue through a workflow action.
    /// </summary>
    /// <param name="actionNameOrId">The workflow action name or id to transition to.</param>
    /// <param name="additionalUpdates">Additional updates to perform when transitioning the issue.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task WorkflowTransitionAsync(string actionNameOrId, WorkflowTransitionUpdates additionalUpdates = null, CancellationToken token = default)
    {
        EnsureCreated("execute workflow transition");

        await _issueService.ExecuteWorkflowActionAsync(this, actionNameOrId, additionalUpdates, token).ConfigureAwait(false);
        await ReloadAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the issues that are marked as sub tasks of this issue.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<Issue> GetSubTasksAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve subtasks");

        return _issueService.GetSubTasksAsync(Key, token);
    }

    /// <summary>
    /// Retrieve attachment metadata from server for this issue
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<Attachment> GetAttachmentsAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve attachments");

        return _issueService.GetAttachmentsAsync(Key, token);
    }

    /// <summary>
    /// Add one or more attachments to this issue
    /// </summary>
    /// <param name="filePaths">Full paths of files to upload</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task AddAttachmentAsync(IEnumerable<string> filePaths, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var attachments = filePaths
            .Select(f => new UploadAttachmentInfo(Path.GetFileName(f), File.ReadAllBytes(f)))
            .ToArray();

        await AddAttachmentAsync(attachments, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Add an attachment to this issue
    /// </summary>
    /// <param name="name">Attachment name with extension</param>
    /// <param name="data">Attachment data</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task AddAttachmentAsync(string name, byte[] data, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(data);

        await AddAttachmentAsync([new UploadAttachmentInfo(name, data)], token).ConfigureAwait(false);
    }

    /// <summary>
    /// Add one or more attachments to this issue.
    /// </summary>
    /// <param name="attachments">Attachment objects that describe the files to upload.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task AddAttachmentAsync(IEnumerable<UploadAttachmentInfo> attachments, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(attachments);
        EnsureCreated("upload attachments to server");

        await _issueService.AddAttachmentsAsync(Key, attachments.ToArray(), token).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes an attachment from this issue.
    /// </summary>
    /// <param name="attachment">Attachment to remove.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task DeleteAttachmentAsync(Attachment attachment, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        EnsureCreated("delete attachment from server");

        await _issueService.DeleteAttachmentAsync(Key, attachment.Model.Id, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a dictionary with issue field names as keys and their metadata as values.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task<IReadOnlyDictionary<string, IssueUpdateMetadata>> GetIssueFieldsEditMetadataAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve issue fields from server");

        return await _issueService.GetFieldsEditMetadataAsync(Key, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve change logs from server for this issue.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<Changelog> GetChangeLogsAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve change logs from server");

        return _issueService.GetChangeLogsAsync(Key, token);
    }

    /// <summary>
    /// Get the comments for this issue.
    /// </summary>
    /// <param name="options">Options to use when querying the comments.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<Comment> GetCommentsAsync(CommentQueryOptions options = null, CancellationToken token = default)
    {
        EnsureCreated("retrieve comments from server");

        return options == null
            ? _issueService.GetCommentsAsync(Key, token)
            : _issueService.GetCommentsAsync(Key, options, token);
    }

    /// <summary>
    /// Add a comment to this issue.
    /// </summary>
    /// <param name="comment">Comment text to add.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task<Comment> AddCommentAsync(string comment, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(comment);

        // The author is not sent, the server sets it to the authenticated user.
        return await AddCommentAsync(new Comment { Body = AdfDocument.FromText(comment) }, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes a comment from this issue.
    /// </summary>
    /// <param name="comment">Comment to remove.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task DeleteCommentAsync(Comment comment, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(comment);
        EnsureCreated("delete comment from server");

        await _issueService.DeleteCommentAsync(Key, comment.Id, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Add a comment to this issue.
    /// </summary>
    /// <param name="comment">Comment object to add.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task<Comment> AddCommentAsync(Comment comment, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(comment);
        EnsureCreated("add comment to issue");

        return await _issueService.AddCommentAsync(Key, comment, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Update a comment in this issue.
    /// </summary>
    /// <param name="comment">Comment object to update.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task<Comment> UpdateCommentAsync(Comment comment, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(comment);
        EnsureCreated("update comment in issue");

        return await _issueService.UpdateCommentAsync(Key, comment, token).ConfigureAwait(false);
    }

    /// <summary>
    ///  Adds a worklog to this issue.
    /// </summary>
    /// <param name="timespent">Specifies a time duration in JIRA duration format, representing the time spent working on the worklog</param>
    /// <param name="worklogStrategy">How to handle the remaining estimate, defaults to AutoAdjustRemainingEstimate</param>
    /// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
    /// <param name="token">Cancellation token for this operation.</param>
    /// <returns>Worklog as constructed by server</returns>
    public async Task<Worklog> AddWorklogAsync(string timespent,
                             WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate,
                             string newEstimate = null,
                             CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(timespent);

        var worklog = new Worklog { TimeSpent = timespent, Started = DateTimeOffset.Now };
        return await AddWorklogAsync(worklog, worklogStrategy, newEstimate, token).ConfigureAwait(false);
    }

    /// <summary>
    ///  Adds a worklog to this issue.
    /// </summary>
    /// <param name="worklog">The worklog instance to add</param>
    /// <param name="worklogStrategy">How to handle the remaining estimate, defaults to AutoAdjustRemainingEstimate</param>
    /// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
    /// <param name="token">Cancellation token for this operation.</param>
    /// <returns>Worklog as constructed by server</returns>
    public async Task<Worklog> AddWorklogAsync(Worklog worklog,
                              WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate,
                              string newEstimate = null,
                              CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(worklog);
        EnsureCreated("add worklog to issue");

        return await _issueService.AddWorklogAsync(Key, worklog, worklogStrategy, newEstimate, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the given worklog from the issue and updates the remaining estimate field.
    /// </summary>
    /// <param name="worklog">The worklog to remove.</param>
    /// <param name="worklogStrategy">How to handle the remaining estimate, defaults to AutoAdjustRemainingEstimate.</param>
    /// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task DeleteWorklogAsync(Worklog worklog, WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate, string newEstimate = null, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(worklog);
        EnsureCreated("delete worklog from issue");

        await _issueService.DeleteWorklogAsync(Key, worklog.Id, worklogStrategy, newEstimate, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve worklogs for this issue.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<Worklog> GetWorklogsAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve worklogs");

        return _issueService.GetWorklogsAsync(Key, token);
    }

    /// <summary>
    /// Updates all fields from server.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task RefreshAsync(CancellationToken token = default)
    {
        EnsureCreated("refresh");

        await ReloadAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the workflow actions that the issue can be transitioned to.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<IssueTransition> GetAvailableActionsAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve actions");

        return _issueService.GetActionsAsync(Key, token);
    }

    /// <summary>
    /// Gets the workflow actions that the issue can be transitioned to including the fields that are required per action.
    /// </summary>
    /// <param name="expandTransitionFields">Whether to show the transition fields.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<IssueTransition> GetAvailableActionsAsync(bool expandTransitionFields, CancellationToken token = default)
    {
        EnsureCreated("retrieve actions");

        return _issueService.GetActionsAsync(Key, expandTransitionFields, token);
    }

    /// <summary>
    /// Gets time tracking information for this issue.
    /// </summary>
    /// <remarks>
    /// - Always retrieves the latest data from the server.
    /// - Use the AddWorklog methods to edit the time tracking information.
    /// </remarks>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task<TimeTrackingDetails> GetTimeTrackingDataAsync(CancellationToken token = default)
    {
        EnsureCreated("retrieve time tracking data");

        return await _issueService.GetTimeTrackingDataAsync(Key, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a user to the watchers of the issue.
    /// </summary>
    /// <param name="usernameOrAccountId">Username or account id of the user to add as a watcher.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task AddWatcherAsync(string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);
        EnsureCreated("add watcher");

        await _issueService.AddWatcherAsync(Key, usernameOrAccountId, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the users that are watching the issue.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<User> GetWatchersAsync(CancellationToken token = default)
    {
        EnsureCreated("get watchers");

        return _issueService.GetWatchersAsync(Key, token);
    }

    /// <summary>
    /// Removes a user from the watchers of the issue.
    /// </summary>
    /// <param name="usernameOrAccountId">Username or account id of the user to remove as a watcher.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task DeleteWatcherAsync(string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);
        EnsureCreated("remove watcher");

        await _issueService.DeleteWatcherAsync(Key, usernameOrAccountId, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Assigns this issue to the specified user.
    /// </summary>
    /// <param name="assignee">The username or account id of the user to assign this issue to.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task AssignAsync(string assignee, CancellationToken token = default)
    {
        EnsureCreated("assign issue");

        await _issueService.AssignIssueAsync(Key, assignee, token).ConfigureAwait(false);
        await ReloadAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches the requested properties and their mapping.
    /// </summary>
    /// <remarks>
    /// Property keys yielded by <paramref name="propertyKeys"/> must have a length between 0 and 256 (both exclusive).
    /// </remarks>
    /// <param name="propertyKeys">Enumeration of requested property keys.</param>
    /// <param name="token">Asynchronous operation control token.</param>
    /// <returns>A dictionary of property values mapped to their keys.</returns>
    public async Task<IReadOnlyDictionary<string, object>> GetPropertiesAsync(IEnumerable<string> propertyKeys, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(propertyKeys);
        EnsureCreated("fetch issue properties");

        return await _issueService.GetPropertiesAsync(Key, propertyKeys, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the value of the specified property key. The key is created if it didn't already exist.
    /// </summary>
    /// <remarks>
    /// The property key (<paramref name="propertyKey"/>) must have a length between 0 and 256 (both exclusive).
    /// </remarks>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="obj">The value to store.</param>
    /// <param name="token">Asynchronous operation control token.</param>
    public async Task SetPropertyAsync(string propertyKey, object obj, CancellationToken token = default)
    {
        ValidatePropertyKey(propertyKey);
        EnsureCreated("add issue properties");

        await _issueService.SetPropertyAsync(Key, propertyKey, obj, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Remove the specified property key and its stored value.
    /// </summary>
    /// <remarks>
    /// The property key (<paramref name="propertyKey"/>) must have a length between 0 and 256 (both exclusive).
    /// </remarks>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="token">Asynchronous operation control token.</param>
    public async Task DeletePropertyAsync(string propertyKey, CancellationToken token = default)
    {
        ValidatePropertyKey(propertyKey);
        EnsureCreated("remove issue properties");

        await _issueService.DeletePropertyAsync(Key, propertyKey, token).ConfigureAwait(false);
    }

    private void EnsureCreated(string action)
    {
        if (string.IsNullOrEmpty(Key))
        {
            throw new InvalidOperationException($"Unable to {action}, issue has not been created.");
        }
    }

    private async Task ReloadAsync(CancellationToken token)
    {
        var serverIssue = await _issueService.GetIssueAsync(Key, token).ConfigureAwait(false);
        Model = serverIssue.Model;
    }

    private static void ValidatePropertyKey(string propertyKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyKey);

        if (propertyKey.Length > MaxPropertyKeyLength)
        {
            throw new ArgumentException($"The property key cannot be longer than {MaxPropertyKeyLength} characters.", nameof(propertyKey));
        }
    }
}
