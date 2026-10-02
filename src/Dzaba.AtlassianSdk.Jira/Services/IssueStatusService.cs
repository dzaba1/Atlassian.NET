using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly JiraCache _cache;

    public IssueStatusService(IClient clientV3, ILogger<IssueStatusService> logger, JiraCache cache)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);

        _clientV3 = clientV3;
        _logger = logger;
        _cache = cache;
    }

    public async IAsyncEnumerable<StatusDetails> GetStatusesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        if (!_cache.Statuses.Any())
        {
            _logger.LogInformation("Getting all issue statuses");

            var statuses = await _clientV3.GetStatusesAsync(token).ConfigureAwait(false);
            if (statuses != null)
            {
                _cache.Statuses.TryAdd(statuses);
            }
        }

        var values = _cache.Statuses.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public async Task<StatusDetails> GetStatusAsync(string idOrName, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(idOrName);

        var status = _cache.Statuses.Values
            .FirstOrDefault(s => string.Equals(idOrName, s.Id, StringComparison.InvariantCulture) || string.Equals(idOrName, s.Name, StringComparison.InvariantCulture));

        if (status == null)
        {
            _logger.LogInformation("Getting issue status {IdOrName}", idOrName);

            status = await _clientV3.GetStatusAsync(idOrName, token).ConfigureAwait(false);
        }

        return status;
    }
}