using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace Atlassian.Jira.Remote;

internal class IssueFilterService : IIssueFilterService
{
    private readonly Jira _jira;

    public IssueFilterService(Jira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<JiraFilter> GetFavouritesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        var array = await _jira.RestClient.ExecuteRequestAsync<JiraFilter[]>(Method.GET, "rest/api/2/filter/favourite", null, token);
        foreach (var item in array)
        {
            yield return item;
        }
    }

    public Task<JiraFilter> GetFilterAsync(string filterId, CancellationToken token = default)
    {
        return _jira.RestClient.ExecuteRequestAsync<JiraFilter>(Method.GET, $"rest/api/2/filter/{filterId}", null, token);
    }

    public async IAsyncEnumerable<Issue> GetIssuesFromFavoriteAsync(string filterName, [EnumeratorCancellation] CancellationToken token = default)
    {
        var jql = await GetFilterJqlByNameAsync(filterName, token).ConfigureAwait(false);

        await foreach (var item in _jira.Issues.GetIssuesFromJqlAsync(jql, token).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<Issue> GetIssuesFromFavoriteWithFieldsAsync(string filterName, IList<string> fields = default, [EnumeratorCancellation] CancellationToken token = default)
    {
        var jql = await GetFilterJqlByNameAsync(filterName, token).ConfigureAwait(false);

        var searchOptions = new IssueSearchOptions(jql)
        {
            AdditionalFields = fields,
            FetchBasicFields = false
        };

        await foreach (var item in _jira.Issues.GetIssuesFromJqlAsync(searchOptions, token).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<Issue> GetIssuesFromFilterAsync(string filterId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var jql = await GetFilterJqlByIdAsync(filterId, token).ConfigureAwait(false);

        await foreach (var item in _jira.Issues.GetIssuesFromJqlAsync(jql, token).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<Issue> GetIssuesFromFilterWithFieldsAsync(string filterId, IList<string> fields = default, [EnumeratorCancellation] CancellationToken token = default)
    {
        var jql = await GetFilterJqlByIdAsync(filterId, token).ConfigureAwait(false);

        var searchOptions = new IssueSearchOptions(jql)
        {
            AdditionalFields = fields,
            FetchBasicFields = false
        };

        await foreach (var item in _jira.Issues.GetIssuesFromJqlAsync(searchOptions, token).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private async Task<string> GetFilterJqlByNameAsync(string filterName, CancellationToken token = default)
    {
        var filters = GetFavouritesAsync(token);
        var filter = await filters.FirstOrDefaultAsync(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);

        if (filter == null)
        {
            throw new InvalidOperationException($"Filter with name '{filterName}' was not found.");
        }

        return filter.Jql;
    }

    private async Task<string> GetFilterJqlByIdAsync(string filterId, CancellationToken token = default)
    {
        var filter = await GetFilterAsync(filterId, token);

        if (filter == null)
        {
            throw new InvalidOperationException($"Filter with ID '{filterId}' was not found.");
        }

        return filter.Jql;
    }
}
