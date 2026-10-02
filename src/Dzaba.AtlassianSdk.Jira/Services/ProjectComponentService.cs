using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations for the project components.
/// </summary>
public interface IProjectComponentService
{
    /// <summary>
    /// Creates a new project component.
    /// </summary>
    /// <param name="projectComponent">Information of the new component.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<ProjectComponent> CreateComponentAsync(ProjectComponent projectComponent, CancellationToken token = default);

    /// <summary>
    /// Deletes a project component.
    /// </summary>
    /// <param name="componentId">Identifier of the component to delete.</param>
    /// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task DeleteComponentAsync(string componentId, string moveIssuesTo = null, CancellationToken token = default);

    /// <summary>
    /// Gets the components for a given project.
    /// </summary>
    /// <param name="projectKey">Key of the project to retrieve the components from.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<ProjectComponent> GetComponentsAsync(string projectKey, CancellationToken token = default);
}

internal sealed class ProjectComponentService : IProjectComponentService
{
    private readonly IClient _clientV3;
    private readonly ILogger<ProjectComponentService> _logger;

    public ProjectComponentService(IClient clientV3, ILogger<ProjectComponentService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async Task<ProjectComponent> CreateComponentAsync(ProjectComponent projectComponent, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(projectComponent);

        _logger.LogInformation("Creating component {Component} in project {ProjectKey}", projectComponent.Name, projectComponent.Project);

        return await _clientV3.CreateComponentAsync(projectComponent, token).ConfigureAwait(false);
    }

    public async Task DeleteComponentAsync(string componentId, string moveIssuesTo = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(componentId);

        _logger.LogInformation("Deleting component {ComponentId} with moving issues to {MoveIssuesTo}", componentId, moveIssuesTo);

        var moveTo = string.IsNullOrEmpty(moveIssuesTo) ? null : moveIssuesTo;
        await _clientV3.DeleteComponentAsync(componentId, moveTo, token).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<ProjectComponent> GetComponentsAsync(string projectKey, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        _logger.LogInformation("Getting components of project {ProjectKey}", projectKey);

        var components = await _clientV3.GetProjectComponentsAsync(projectKey, null, token).ConfigureAwait(false);
        if (components == null)
        {
            yield break;
        }

        foreach (var component in components)
        {
            yield return component;
        }
    }
}
