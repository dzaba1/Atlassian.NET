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
/// Represents the operations on the filters of jira.
/// </summary>
public interface IIssueFilterService
{
    /// <summary>
    /// Returns a filter with the specified id.
    /// </summary>
    /// <param name="filterId">Identifier of the filter to fetch.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<Filter> GetFilterAsync(long filterId, CancellationToken token = default);

    /// <summary>
    /// Returns the favourite filters for the user.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Filter> GetFavouritesAsync(CancellationToken token = default);

    /// <summary>
    /// Returns issues that match the specified favorite filter.
    /// </summary>
    /// <param name="filterName">The name of the filter used for the search</param>
    /// <param name="token">Cancellation token for this operation.</param>
    /// <remarks>Includes basic fields.</remarks>
    IAsyncEnumerable<Issue> GetIssuesFromFavoriteAsync(string filterName, CancellationToken token = default);

    /// <summary>
    /// Returns issues that match the specified favorite filter.
    /// </summary>
    /// <param name="filterName">The name of the filter used for the search</param>
    /// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Issue> GetIssuesFromFavoriteWithFieldsAsync(string filterName, IEnumerable<string> fields = default, CancellationToken token = default);

    /// <summary>
    /// Returns issues that match the filter with the specified id.
    /// </summary>
    /// <param name="filterId">Identifier of the filter to fetch.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    /// <remarks>Includes basic fields.</remarks>
    IAsyncEnumerable<Issue> GetIssuesFromFilterAsync(long filterId, CancellationToken token = default);

    /// <summary>
    /// Returns issues that match the filter with the specified id.
    /// </summary>
    /// <param name="filterId">Identifier of the filter to fetch.</param>
    /// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Issue> GetIssuesFromFilterWithFieldsAsync(long filterId, IEnumerable<string> fields = default, CancellationToken token = default);
}

internal sealed class IssueFilterService : IIssueFilterService
{
    private readonly IClient _clientV3;
    private readonly ILogger<IssueFilterService> _logger;
    private readonly IIssueService _issueService;

    public IssueFilterService(IClient clientV3,
        ILogger<IssueFilterService> logger,
        IIssueService issueService)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(issueService);

        _clientV3 = clientV3;
        _logger = logger;
        _issueService = issueService;
    }

    public async Task<Filter> GetFilterAsync(long filterId, CancellationToken token = default)
    {
        _logger.LogInformation("Getting filter {FilterId}", filterId);

        return await _clientV3.GetFilterAsync(filterId, null, null, token).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<Filter> GetFavouritesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        _logger.LogInformation("Getting favourite filters");

        // The endpoint is not paged, it returns all the favourite filters of the user at once.
        var filters = await _clientV3.GetFavouriteFiltersAsync(null, token).ConfigureAwait(false);
        if (filters == null)
        {
            yield break;
        }

        foreach (var filter in filters)
        {
            yield return filter;
        }
    }

    public IAsyncEnumerable<Issue> GetIssuesFromFavoriteAsync(string filterName, CancellationToken token = default)
    {
        return GetIssuesFromFavoriteWithFieldsAsync(filterName, null, true, token);
    }

    public IAsyncEnumerable<Issue> GetIssuesFromFavoriteWithFieldsAsync(string filterName, IEnumerable<string> fields = default, CancellationToken token = default)
    {
        return GetIssuesFromFavoriteWithFieldsAsync(filterName, fields, false, token);
    }

    public IAsyncEnumerable<Issue> GetIssuesFromFilterAsync(long filterId, CancellationToken token = default)
    {
        return GetIssuesFromFilterWithFieldsAsync(filterId, null, true, token);
    }

    public IAsyncEnumerable<Issue> GetIssuesFromFilterWithFieldsAsync(long filterId, IEnumerable<string> fields = default, CancellationToken token = default)
    {
        return GetIssuesFromFilterWithFieldsAsync(filterId, fields, false, token);
    }

    private async IAsyncEnumerable<Issue> GetIssuesFromFavoriteWithFieldsAsync(string filterName, IEnumerable<string> fields, bool fetchBasicFields, [EnumeratorCancellation] CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(filterName);

        var jql = await GetFilterJqlByNameAsync(filterName, token).ConfigureAwait(false);

        await foreach (var issue in SearchAsync(jql, fields, fetchBasicFields, token).WithCancellation(token).ConfigureAwait(false))
        {
            yield return issue;
        }
    }

    private async IAsyncEnumerable<Issue> GetIssuesFromFilterWithFieldsAsync(long filterId, IEnumerable<string> fields, bool fetchBasicFields, [EnumeratorCancellation] CancellationToken token)
    {
        var jql = await GetFilterJqlByIdAsync(filterId, token).ConfigureAwait(false);

        await foreach (var issue in SearchAsync(jql, fields, fetchBasicFields, token).WithCancellation(token).ConfigureAwait(false))
        {
            yield return issue;
        }
    }

    private IAsyncEnumerable<Issue> SearchAsync(string jql, IEnumerable<string> fields, bool fetchBasicFields, CancellationToken token)
    {
        var options = new IssueSearchOptions(jql)
        {
            AdditionalFields = fields?.ToList() ?? [],
            FetchBasicFields = fetchBasicFields
        };

        return _issueService.GetIssuesFromJqlAsync(options, token);
    }

    private async Task<string> GetFilterJqlByNameAsync(string filterName, CancellationToken token)
    {
        await foreach (var filter in GetFavouritesAsync(token).WithCancellation(token).ConfigureAwait(false))
        {
            if (string.Equals(filter.Name, filterName, StringComparison.OrdinalIgnoreCase))
            {
                return filter.Jql;
            }
        }

        throw new InvalidOperationException($"Filter with name '{filterName}' was not found.");
    }

    private async Task<string> GetFilterJqlByIdAsync(long filterId, CancellationToken token)
    {
        var filter = await GetFilterAsync(filterId, token).ConfigureAwait(false);

        if (filter == null)
        {
            throw new InvalidOperationException($"Filter with ID '{filterId}' was not found.");
        }

        return filter.Jql;
    }
}