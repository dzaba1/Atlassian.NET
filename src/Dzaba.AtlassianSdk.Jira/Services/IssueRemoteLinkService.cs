using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the remote links of a jira issue.
/// </summary>
public interface IIssueRemoteLinkService
{
    /// <summary>
    /// Creates an remote link for an issue.
    /// </summary>
    /// <param name="issueKey">Key of the issue.</param>
    /// <param name="remoteUrl">Remote url to link to.</param>
    /// <param name="title">Title of the remote link.</param>
    /// <param name="summary">Summary of the remote link.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task CreateRemoteLinkAsync(string issueKey, string remoteUrl, string title, string summary, CancellationToken token = default);

    /// <summary>
    /// Returns all remote links associated with a given issue.
    /// </summary>
    /// <param name="issueKey">The issue to retrieve remote links for.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<RemoteIssueLink> GetRemoteLinksForIssueAsync(string issueKey, CancellationToken token = default);
}

internal sealed class IssueRemoteLinkService : IIssueRemoteLinkService
{
    private readonly IClient _clientV3;
    private readonly ILogger<IssueRemoteLinkService> _logger;

    public IssueRemoteLinkService(IClient clientV3, ILogger<IssueRemoteLinkService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async Task CreateRemoteLinkAsync(string issueKey, string remoteUrl, string title, string summary, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);
        ArgumentException.ThrowIfNullOrEmpty(remoteUrl);
        ArgumentException.ThrowIfNullOrEmpty(title);

        _logger.LogInformation("Creating remote link {RemoteUrl} for issue {IssueKey}", remoteUrl, issueKey);

        var body = new RemoteIssueLinkRequest
        {
            Object = new RemoteObject
            {
                Title = title,
                Url = remoteUrl,
                Summary = string.IsNullOrEmpty(summary) ? null : summary
            }
        };
        await _clientV3.CreateOrUpdateRemoteIssueLinkAsync(issueKey, body, token).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<RemoteIssueLink> GetRemoteLinksForIssueAsync(string issueKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        _logger.LogInformation("Getting remote links of issue {IssueKey}", issueKey);

        var links = await _clientV3.GetRemoteIssueLinksAsync(issueKey, null, token).ConfigureAwait(false);
        if (links == null)
        {
            yield break;
        }

        foreach (var link in links)
        {
            yield return link;
        }
    }
}