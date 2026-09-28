using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Atlassian.Jira.Remote;
using RestSharp;

namespace Atlassian.Jira.Services;

internal sealed class IssuePriorityService : IIssuePriorityService
{
    private readonly IJira _jira;

    public IssuePriorityService(IJira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<IssuePriority> GetPrioritiesAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;

        if (!cache.Priorities.Any())
        {
            var priorities = await _jira.RestClient.ExecuteRequestAsync<RemotePriority[]>(Method.Get, "rest/api/2/priority", null, token).ConfigureAwait(false);
            cache.Priorities.TryAdd(priorities.Select(p => new IssuePriority(p)));
        }

        var values = cache.Priorities.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }
}
