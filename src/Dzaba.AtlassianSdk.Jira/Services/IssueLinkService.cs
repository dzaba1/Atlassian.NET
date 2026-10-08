using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the issue link of jira.
/// </summary>
public interface IIssueLinkService
{
    /// <summary>
    /// Returns all available issue link types.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<IssueLinkType> GetLinkTypesAsync(CancellationToken token = default);

    /// <summary>
    /// Creates an issue link between two issues.
    /// </summary>
    /// <param name="outwardIssueKey">Key of the outward issue.</param>
    /// <param name="inwardIssueKey">Key of the inward issue.</param>
    /// <param name="linkName">Name of the issue link.</param>
    /// <param name="comment">Comment to add to the outward issue.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task CreateLinkAsync(string outwardIssueKey, string inwardIssueKey, string linkName, string comment, CancellationToken token = default);

    /// <summary>
    /// Returns all issue links associated with a given issue.
    /// </summary>
    /// <param name="issueKey">The issue key to retrieve links for.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<IssueLink> GetLinksForIssueAsync(string issueKey, CancellationToken token = default);

    /// <summary>
    /// Returns all issue links associated with a given issue.
    /// </summary>
    /// <param name="issue">The issue to retrieve links for.</param>
    /// <param name="linkTypeNames">Optional subset of link types to retrieve.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<IssueLink> GetLinksForIssueAsync(Issue issue, IEnumerable<string> linkTypeNames = null, CancellationToken token = default);
}

internal sealed class IssueLinkService : IIssueLinkService
{
    private readonly IClient _clientV3;
    private readonly ILogger<IssueLinkService> _logger;
    private readonly JiraCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public IssueLinkService(IClient clientV3, ILogger<IssueLinkService> logger, JiraCache cache)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);

        _clientV3 = clientV3;
        _logger = logger;
        _cache = cache;
    }

    public async IAsyncEnumerable<IssueLinkType> GetLinkTypesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        if (!_cache.LinkTypes.Any())
        {
            _logger.LogInformation("Getting all issue link types");

            var result = await _clientV3.GetIssueLinkTypesAsync(token).ConfigureAwait(false);
            if (result?.IssueLinkTypes1 != null)
            {
                _cache.LinkTypes.TryAdd(result.IssueLinkTypes1);
            }
        }

        var values = _cache.LinkTypes.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public async Task CreateLinkAsync(string outwardIssueKey, string inwardIssueKey, string linkName, string comment, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(outwardIssueKey);
        ArgumentException.ThrowIfNullOrEmpty(inwardIssueKey);
        ArgumentException.ThrowIfNullOrEmpty(linkName);

        _logger.LogInformation("Creating issue link {LinkName} from {OutwardIssueKey} to {InwardIssueKey}", linkName, outwardIssueKey, inwardIssueKey);

        var body = new LinkIssueRequestJsonBean
        {
            Type = new IssueLinkType { Name = linkName },
            InwardIssue = new LinkedIssue { Key = inwardIssueKey },
            OutwardIssue = new LinkedIssue { Key = outwardIssueKey }
        };

        if (!string.IsNullOrEmpty(comment))
        {
            body.Comment = new Comment { Body = CreateAdfBody(comment) };
        }

        await _clientV3.LinkIssuesAsync(body, token).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<IssueLink> GetLinksForIssueAsync(string issueKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        await foreach (var link in GetLinksAsync(issueKey, null, token).ConfigureAwait(false))
        {
            yield return link;
        }
    }

    public async IAsyncEnumerable<IssueLink> GetLinksForIssueAsync(Issue issue, IEnumerable<string> linkTypeNames = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(issue);

        await foreach (var link in GetLinksAsync(issue.Model.Key, linkTypeNames, token).ConfigureAwait(false))
        {
            yield return link;
        }
    }

    private async IAsyncEnumerable<IssueLink> GetLinksAsync(string issueKey, IEnumerable<string> linkTypeNames, [EnumeratorCancellation] CancellationToken token)
    {
        _logger.LogInformation("Getting issue links of issue {IssueKey}", issueKey);

        var issue = await _clientV3.GetIssueAsync(issueKey, ["issuelinks"], null, null, null, null, null, token).ConfigureAwait(false);
        if (issue?.Fields == null || !issue.Fields.TryGetValue("issuelinks", out var linksValue) || linksValue is not JsonElement linksJson)
        {
            throw new InvalidOperationException("There is no 'issueLinks' field on the issue data, make sure issue linking is turned on in JIRA.");
        }

        var links = linksJson.ValueKind == JsonValueKind.Array
            ? linksJson.Deserialize<List<IssueLink>>(JsonOptions)
            : null;

        if (links == null)
        {
            yield break;
        }

        var thisIssue = new LinkedIssue { Id = issue.Id, Key = issue.Key, Self = issue.Self };

        foreach (var link in links)
        {
            if (linkTypeNames != null && !linkTypeNames.Contains(link.Type?.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            // Jira returns only the other side of the link, the side that is the queried issue is missing.
            if (IsEmpty(link.OutwardIssue))
            {
                link.OutwardIssue = thisIssue;
            }

            if (IsEmpty(link.InwardIssue))
            {
                link.InwardIssue = thisIssue;
            }

            yield return link;
        }
    }

    private static bool IsEmpty(LinkedIssue issue)
    {
        return issue == null || (string.IsNullOrEmpty(issue.Key) && string.IsNullOrEmpty(issue.Id));
    }

    // The V3 API accepts comment bodies only in the Atlassian Document Format.
    private static object CreateAdfBody(string text)
    {
        return new
        {
            type = "doc",
            version = 1,
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    content = new[]
                    {
                        new { type = "text", text }
                    }
                }
            }
        };
    }
}