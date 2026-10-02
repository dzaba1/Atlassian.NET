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
/// Represents the operations on the issue resolutions of jira.
/// </summary>
public interface IIssueResolutionService
{
    /// <summary>
    /// Returns all the issue resolutions within JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<ResolutionJsonBean> GetResolutionsAsync(CancellationToken token = default);
}

internal sealed class IssueResolutionService : IIssueResolutionService
{
    private const int MaxResolutionsResults = 50;

    private readonly IClient _clientV3;
    private readonly ILogger<IssueResolutionService> _logger;
    private readonly JiraCache _cache;

    public IssueResolutionService(IClient clientV3, ILogger<IssueResolutionService> logger, JiraCache cache)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);

        _clientV3 = clientV3;
        _logger = logger;
        _cache = cache;
    }

    public async IAsyncEnumerable<ResolutionJsonBean> GetResolutionsAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        if (!_cache.Resolutions.Any())
        {
            _logger.LogInformation("Getting all issue resolutions");

            var resolutions = PageExpander.ExpandAsync(
                (startAt, maxResults, t) => _clientV3.SearchResolutionsAsync(
                    startAt.ToString(CultureInfo.InvariantCulture), maxResults.ToString(CultureInfo.InvariantCulture), null, null, t),
                page => page.Values,
                page => page.IsLast,
                MaxResolutionsResults,
                token);

            await foreach (var resolution in resolutions.WithCancellation(token).ConfigureAwait(false))
            {
                _cache.Resolutions.TryAdd(resolution);
            }
        }

        var values = _cache.Resolutions.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }
}