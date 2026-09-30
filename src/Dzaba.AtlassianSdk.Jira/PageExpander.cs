using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Fetches a single page from an offset based paged endpoint.
/// </summary>
/// <typeparam name="TPage">Type of the page returned by the endpoint.</typeparam>
/// <param name="startAt">The index of the first item to return.</param>
/// <param name="maxResults">The maximum number of items to return.</param>
/// <param name="token">Cancellation token for this operation.</param>
internal delegate Task<TPage> FetchPageHandler<TPage>(long startAt, int maxResults, CancellationToken token);

/// <summary>
/// Turns offset based paged endpoints (startAt + maxResults) into a single stream of items.
/// </summary>
internal static class PageExpander
{
    /// <summary>
    /// Fetches consecutive pages until the last one and yields all of their items.
    /// </summary>
    /// <typeparam name="TPage">Type of the page returned by the endpoint.</typeparam>
    /// <typeparam name="TItem">Type of the items in the page.</typeparam>
    /// <param name="fetchPage">Fetches a page for the given startAt and maxResults.</param>
    /// <param name="getItems">Selects items from the page. Can return null for an empty page.</param>
    /// <param name="isLast">Tells if the page is the last one.</param>
    /// <param name="pageSize">Number of items requested per page. Must be the maximum the endpoint allows.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public static async IAsyncEnumerable<TItem> ExpandAsync<TPage, TItem>(
        FetchPageHandler<TPage> fetchPage,
        Func<TPage, IEnumerable<TItem>> getItems,
        Func<TPage, bool> isLast,
        int pageSize,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(fetchPage);
        ArgumentNullException.ThrowIfNull(getItems);
        ArgumentNullException.ThrowIfNull(isLast);

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than zero.");
        }

        long startAt = 0;
        bool last;

        do
        {
            token.ThrowIfCancellationRequested();

            var page = await fetchPage(startAt, pageSize, token).ConfigureAwait(false);

            var items = getItems(page);
            if (items != null)
            {
                foreach (var item in items)
                {
                    yield return item;
                }
            }

            // Some endpoints apply the range before filtering, so a page can hold fewer items than requested.
            // The offset has to advance by the page size, not by the number of returned items.
            startAt += pageSize;
            last = isLast(page);
        }
        while (!last);
    }

    /// <summary>
    /// Fetches consecutive pages of an endpoint that returns only a bare collection and yields all of their items.
    /// Such an endpoint gives no "last page" flag and may apply the range before filtering, so a page can hold
    /// fewer items than requested. The only reliable end marker is an empty page.
    /// </summary>
    /// <typeparam name="TItem">Type of the items in the page.</typeparam>
    /// <param name="fetchPage">Fetches a page for the given startAt and maxResults.</param>
    /// <param name="pageSize">Number of items requested per page. Must be the maximum the endpoint allows.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public static IAsyncEnumerable<TItem> ExpandAsync<TItem>(
        FetchPageHandler<ICollection<TItem>> fetchPage,
        int pageSize,
        CancellationToken token = default)
    {
        return ExpandAsync<ICollection<TItem>, TItem>(
            fetchPage,
            page => page,
            page => page == null || page.Count == 0,
            pageSize,
            token);
    }
}
