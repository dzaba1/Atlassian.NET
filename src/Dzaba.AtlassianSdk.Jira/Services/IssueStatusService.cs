using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the issue statuses of jira.
/// </summary>
public interface IIssueStatusService
{
    /// <summary>
    /// Returns all the issue statuses within JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<StatusDetails> GetStatusesAsync(CancellationToken token = default);

    /// <summary>
    /// Returns a full representation of the status having the given id or name.
    /// </summary>
    /// <param name="idOrName">The status identifier or name.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<StatusDetails> GetStatusAsync(string idOrName, CancellationToken token = default);
}

internal sealed class IssueStatusService : IIssueStatusService
{
    private readonly IClient _clientV3;
    private readonly ILogger<IssueStatusService> _logger;

    public IssueStatusService(IClient clientV3, ILogger<IssueStatusService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async IAsyncEnumerable<StatusDetails> GetStatusesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        _logger.LogInformation("Getting all issue statuses");

        var statuses = await _clientV3.GetStatusesAsync(token).ConfigureAwait(false);
        if (statuses == null)
        {
            yield break;
        }

        foreach (var status in statuses)
        {
            yield return status;
        }
    }

    public async Task<StatusDetails> GetStatusAsync(string idOrName, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(idOrName);

        _logger.LogInformation("Getting issue status {IdOrName}", idOrName);

        return await _clientV3.GetStatusAsync(idOrName, token).ConfigureAwait(false);
    }
}