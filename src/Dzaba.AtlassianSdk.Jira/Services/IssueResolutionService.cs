using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
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

    public IssueResolutionService(IClient clientV3, ILogger<IssueResolutionService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public IAsyncEnumerable<ResolutionJsonBean> GetResolutionsAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Getting all issue resolutions");

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.SearchResolutionsAsync(
                startAt.ToString(CultureInfo.InvariantCulture), maxResults.ToString(CultureInfo.InvariantCulture), null, null, t),
            page => page.Values,
            page => page.IsLast,
            MaxResolutionsResults,
            token);
    }
}