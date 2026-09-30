using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the issue types of jira.
/// </summary>
public interface IIssueTypeService
{
    /// <summary>
    /// Returns all the issue types within JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<IssueTypeDetails> GetIssueTypesAsync(CancellationToken token = default);

    /// <summary>
    /// Returns the issue types within JIRA for the project specified.
    /// </summary>
    /// <param name="projectKey">Key of the project to return issue types for.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<IssueTypeWithStatus> GetIssueTypesForProjectAsync(string projectKey, CancellationToken token = default);
}

internal sealed class IssueTypeService : IIssueTypeService
{
    private readonly IClient _clientV3;
    private readonly ILogger<IssueTypeService> _logger;

    public IssueTypeService(IClient clientV3, ILogger<IssueTypeService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async IAsyncEnumerable<IssueTypeDetails> GetIssueTypesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        _logger.LogInformation("Getting all issue types");

        var issueTypes = await _clientV3.GetIssueAllTypesAsync(token).ConfigureAwait(false);
        if (issueTypes == null)
        {
            yield break;
        }

        foreach (var issueType in issueTypes)
        {
            yield return issueType;
        }
    }

    public async IAsyncEnumerable<IssueTypeWithStatus> GetIssueTypesForProjectAsync(string projectKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        _logger.LogInformation("Getting issue types of project {ProjectKey}", projectKey);

        var issueTypes = await _clientV3.GetAllStatusesAsync(projectKey, token).ConfigureAwait(false);
        if (issueTypes == null)
        {
            yield break;
        }

        foreach (var issueType in issueTypes)
        {
            yield return issueType;
        }
    }
}
