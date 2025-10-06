using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Remote;

internal class ProjectService : IProjectService
{
    private readonly Jira _jira;

    public ProjectService(Jira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<Project> GetProjectsAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;
        if (!cache.Projects.Any())
        {
            var remoteProjects = await _jira.RestClient.ExecuteRequestAsync<RemoteProject[]>(Method.GET, "rest/api/2/project?expand=lead,url", null, token).ConfigureAwait(false);
            cache.Projects.TryAdd(remoteProjects.Select(p => new Project(_jira, p)));
        }

        var values = cache.Projects.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public async Task<Project> GetProjectAsync(string projectKey, CancellationToken token = new CancellationToken())
    {
        var resource = string.Format("rest/api/2/project/{0}?expand=lead,url", projectKey);
        var remoteProject = await _jira.RestClient.ExecuteRequestAsync<RemoteProject>(Method.GET, resource, null, token).ConfigureAwait(false);
        return new Project(_jira, remoteProject);
    }

    public Task DeleteProjectAsync(string projectKey, CancellationToken token = default)
    {
        var resource = string.Format("rest/api/2/project/{0}", projectKey);
        return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);
    }

    public async Task<Project> CreateProjectAsync(NewProject project, CancellationToken token = default)
    {
        var requestBody = JsonConvert.SerializeObject(project, _jira.RestClient.Settings.JsonSerializerSettings);

        await _jira.RestClient.ExecuteRequestAsync(Method.POST, "rest/api/3/project", requestBody, token).ConfigureAwait(false);
        return await GetProjectAsync(project.Key);
    }
}
