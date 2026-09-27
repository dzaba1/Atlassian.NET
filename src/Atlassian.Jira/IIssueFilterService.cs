using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira;

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
    Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken token = default);

    /// <summary>
    /// Returns the favourite filters for the user.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<JiraFilter> GetFavouritesAsync(CancellationToken token = default);

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
    IAsyncEnumerable<Issue> GetIssuesFromFavoriteWithFieldsAsync(string filterName, IList<string> fields = default, CancellationToken token = default);

    /// <summary>
    /// Returns issues that match the filter with the specified id.
    /// </summary>
    /// <param name="filterId">Identifier of the filter to fetch.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    /// <remarks>Includes basic fields.</remarks>
    IAsyncEnumerable<Issue> GetIssuesFromFilterAsync(string filterId, CancellationToken token = default);

    /// <summary>
    /// Returns issues that match the filter with the specified id.
    /// </summary>
    /// <param name="filterId">Identifier of the filter to fetch.</param>
    /// <param name="fields">A list of specific fields to fetch. Empty or <see langword="null"/> will fetch all fields.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Issue> GetIssuesFromFilterWithFieldsAsync(string filterId, IList<string> fields = default, CancellationToken token = default);
}
