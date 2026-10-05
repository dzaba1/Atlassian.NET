using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Dzaba.AtlassianSdk.Jira.Services;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A JIRA project
/// </summary>
public class Project : IJiraEntity
{
    private readonly IIssueTypeService _issueTypeService;
    private readonly IProjectComponentService _componentService;
    private readonly IProjectVersionService _versionService;

    public Project(IIssueTypeService issueTypeService,
        IProjectComponentService componentService,
        IProjectVersionService versionService)
    {
        ArgumentNullException.ThrowIfNull(issueTypeService);
        ArgumentNullException.ThrowIfNull(componentService);
        ArgumentNullException.ThrowIfNull(versionService);

        _issueTypeService = issueTypeService;
        _componentService = componentService;
        _versionService = versionService;
    }

    /// <summary>
    /// Id of the entity.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Name of the entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The unique identifier of the project.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The category set on this project.
    /// </summary>
    public ProjectCategory Category { get; set; }

    /// <summary>
    /// User object of the project lead.
    /// </summary>
    public User LeadUser { get; set; }

    /// <summary>
    /// The URL set on the project.
    /// </summary>
    public Uri Url { get; set; }

    /// <summary>
    /// The list of the Avatar URL's
    /// </summary>
    public AvatarUrlsBean AvatarUrls { get; set; }

    /// <summary>
    /// Gets the issue types for the current project.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<IssueTypeWithStatus> GetIssueTypesAsync(CancellationToken token = default)
    {
        return _issueTypeService.GetIssueTypesForProjectAsync(Key, token);
    }

    /// <summary>
    /// Creates a new project component.
    /// </summary>
    /// <param name="projectComponent">Information of the new component.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public Task<ProjectComponent> AddComponentAsync(ProjectComponent projectComponent, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(projectComponent);

        projectComponent.Project = Key;
        return _componentService.CreateComponentAsync(projectComponent, token);
    }

    /// <summary>
    /// Gets the components for the current project.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<ProjectComponent> GetComponentsAsync(CancellationToken token = default)
    {
        return _componentService.GetComponentsAsync(Key, token);
    }

    /// <summary>
    /// Deletes a project component.
    /// </summary>
    /// <param name="componentName">Name of the component to remove.</param>
    /// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task DeleteComponentAsync(string componentName, string moveIssuesTo = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(componentName);

        var component = await GetComponentsAsync(token)
            .FirstOrDefaultAsync(c => string.Equals(c.Name, componentName), token)
            .ConfigureAwait(false);

        if (component == null)
        {
            throw new InvalidOperationException($"Unable to locate a component with name '{componentName}'");
        }

        await _componentService.DeleteComponentAsync(component.Id, moveIssuesTo, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new project version.
    /// </summary>
    /// <param name="projectVersion">Information of the new project version.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public Task<Model.V3.Version> AddVersionAsync(Model.V3.Version projectVersion, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(projectVersion);

        projectVersion.Project = Key;
        return _versionService.CreateVersionAsync(projectVersion, token);
    }

    /// <summary>
    /// Gets the versions for this project.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public IAsyncEnumerable<Model.V3.Version> GetVersionsAsync(CancellationToken token = default)
    {
        return _versionService.GetVersionsAsync(Key, token);
    }

    /// <summary>
    /// Deletes a project version.
    /// </summary>
    /// <param name="versionName">Name of the version to delete.</param>
    /// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
    /// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task DeleteVersionAsync(string versionName, string moveFixIssuesTo = null, string moveAffectedIssuesTo = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionName);

        var version = await GetVersionsAsync(token)
            .FirstOrDefaultAsync(v => string.Equals(v.Name, versionName, StringComparison.OrdinalIgnoreCase), token)
            .ConfigureAwait(false);

        if (version == null)
        {
            throw new InvalidOperationException($"Unable to locate a version with name '{versionName}'");
        }

        await _versionService.DeleteVersionAsync(version.Id, moveFixIssuesTo, moveAffectedIssuesTo, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the name of the entity, or its id if the name is not set.
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }
        else
        {
            return Id;
        }
    }
}