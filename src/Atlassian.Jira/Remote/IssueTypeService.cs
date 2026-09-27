using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Atlassian.Jira.Remote;

internal class IssueTypeService : IIssueTypeService
{
    private readonly Jira _jira;

    public IssueTypeService(Jira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<IssueType> GetIssueTypesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;

        if (!cache.IssueTypes.Any())
        {
            var remoteIssueTypes = await _jira.RestClient.ExecuteRequestAsync<RemoteIssueType[]>(Method.Get, "rest/api/2/issuetype", null, token).ConfigureAwait(false);
            var issueTypes = remoteIssueTypes.Select(t => new IssueType(t));
            cache.IssueTypes.TryAdd(issueTypes);
        }

        var values = cache.IssueTypes.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public async IAsyncEnumerable<IssueType> GetIssueTypesForProjectAsync(string projectKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;

        if (!cache.ProjectIssueTypes.TryGetValue(projectKey, out JiraEntityDictionary<IssueType> _))
        {
            var resource = string.Format("rest/api/2/project/{0}/statuses", projectKey);
            var results = await _jira.RestClient.ExecuteRequestAsync<RemoteIssueType[]>(Method.Get, resource, null, token).ConfigureAwait(false);
            var issueTypes = results.Select(x => new IssueType(x));

            cache.ProjectIssueTypes.TryAdd(projectKey, new JiraEntityDictionary<IssueType>(issueTypes));
        }

        var values = cache.ProjectIssueTypes[projectKey].Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }
}
