using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Atlassian.Jira.Remote;

internal class ProjectComponentService : IProjectComponentService
{
    private readonly Jira _jira;

    public ProjectComponentService(Jira jira)
    {
        _jira = jira;

    }

    public async Task<ProjectComponent> CreateComponentAsync(ProjectComponentCreationInfo projectComponent, CancellationToken token = default)
    {
        var serializer = JsonSerializer.Create(_jira.RestClient.Settings.JsonSerializerSettings);
        var resource = "/rest/api/2/component";
        var requestBody = JToken.FromObject(projectComponent, serializer);
        var remoteComponent = await _jira.RestClient.ExecuteRequestAsync<RemoteComponent>(Method.Post, resource, requestBody, token).ConfigureAwait(false);
        remoteComponent.ProjectKey = projectComponent.ProjectKey;
        var component = new ProjectComponent(remoteComponent);

        _jira.Cache.Components.TryAdd(component);

        return component;
    }

    public async Task DeleteComponentAsync(string componentId, string moveIssuesTo = null, CancellationToken token = default)
    {
        var resource = string.Format("/rest/api/2/component/{0}?{1}",
            componentId,
            string.IsNullOrEmpty(moveIssuesTo) ? null : "moveIssuesTo=" + Uri.EscapeDataString(moveIssuesTo));

        await _jira.RestClient.ExecuteRequestAsync(Method.Delete, resource, null, token).ConfigureAwait(false);

        _jira.Cache.Components.TryRemove(componentId);
    }

    public async IAsyncEnumerable<ProjectComponent> GetComponentsAsync(string projectKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;
        IEnumerable<ProjectComponent> result;

        if (!cache.Components.Values.Any(c => string.Equals(c.ProjectKey, projectKey)))
        {
            var resource = string.Format("rest/api/2/project/{0}/components", projectKey);
            var remoteComponents = await _jira.RestClient.ExecuteRequestAsync<RemoteComponent[]>(Method.Get, resource).ConfigureAwait(false);
            var components = remoteComponents.Select(remoteComponent =>
            {
                remoteComponent.ProjectKey = projectKey;
                return new ProjectComponent(remoteComponent);
            });
            cache.Components.TryAdd(components);
            result = components;
        }
        else
        {
            result = cache.Components.Values.Where(c => string.Equals(c.ProjectKey, projectKey));
        }

        foreach (var value in result)
        {
            yield return value;
        }
    }
}
