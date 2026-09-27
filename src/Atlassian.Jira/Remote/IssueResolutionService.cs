using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Atlassian.Jira.Remote;

internal class IssueResolutionService : IIssueResolutionService
{
    private readonly Jira _jira;

    public IssueResolutionService(Jira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<IssueResolution> GetResolutionsAsync([EnumeratorCancellation] CancellationToken token)
    {
        var cache = _jira.Cache;

        if (!cache.Resolutions.Any())
        {
            var resolutions = await _jira.RestClient.ExecuteRequestAsync<RemoteResolution[]>(Method.Get, "rest/api/2/resolution", null, token).ConfigureAwait(false);
            cache.Resolutions.TryAdd(resolutions.Select(r => new IssueResolution(r)));
        }

        var values = cache.Resolutions.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }
}
