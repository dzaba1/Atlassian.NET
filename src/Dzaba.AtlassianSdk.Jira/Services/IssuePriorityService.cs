using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the issue priorities of jira.
/// </summary>
public interface IIssuePriorityService
{
    /// <summary>
    /// Returns all the issue priorities within JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Priority> GetPrioritiesAsync(CancellationToken token = default);
}

internal sealed class IssuePriorityService : IIssuePriorityService
{
    private const int MaxPrioritiesResults = 50;

    private readonly IClient _clientV3;
    private readonly ILogger<IssuePriorityService> _logger;
    private readonly JiraCache _cache;

    public IssuePriorityService(IClient clientV3, ILogger<IssuePriorityService> logger, JiraCache cache)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);

        _clientV3 = clientV3;
        _logger = logger;
        _cache = cache;
    }

    public async IAsyncEnumerable<Priority> GetPrioritiesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        if (!_cache.Priorities.Any())
        {
            _logger.LogInformation("Getting all issue priorities");

            var priorities = PageExpander.ExpandAsync(
                (startAt, maxResults, t) => _clientV3.SearchPrioritiesAsync(
                    startAt.ToString(CultureInfo.InvariantCulture), maxResults.ToString(CultureInfo.InvariantCulture), null, null, null, null, null, t),
                page => page.Values,
                page => page.IsLast,
                MaxPrioritiesResults,
                token);

            await foreach (var priority in priorities.WithCancellation(token).ConfigureAwait(false))
            {
                _cache.Priorities.TryAdd(priority);
            }
        }

        var values = _cache.Priorities.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }
}