using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the projects of jira.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Returns all projects defined in JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Project> GetProjectsAsync(CancellationToken token = default);

    /// <summary>
    /// Returns a single project in JIRA.
    /// </summary>
    /// <param name="projectKey">Project key for the single project to load</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<Project> GetProjectAsync(string projectKey, CancellationToken token = default);

    /// <summary>
    /// Deletes the specified project.
    /// </summary>
    /// <param name="projectKey">Key of project to delete.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task DeleteProjectAsync(string projectKey, CancellationToken token = default);

    /// <summary>
    /// Creates a project.
    /// </summary>
    /// <param name="project">Project to create.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<Project> CreateProjectAsync(CreateProjectDetails project, CancellationToken token = default);
}

internal sealed class ProjectService : IProjectService
{
    private const int MaxProjectsResults = 50;
    private const string ProjectExpand = "lead,url";

    private readonly IClient _clientV3;
    private readonly ILogger<ProjectService> _logger;
    private readonly JiraCache _cache;
    private readonly IIssueTypeService _issueTypeService;
    private readonly IProjectComponentService _componentService;
    private readonly IProjectVersionService _versionService;

    public ProjectService(IClient clientV3,
        ILogger<ProjectService> logger,
        JiraCache cache,
        IIssueTypeService issueTypeService,
        IProjectComponentService componentService,
        IProjectVersionService versionService)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(issueTypeService);
        ArgumentNullException.ThrowIfNull(componentService);
        ArgumentNullException.ThrowIfNull(versionService);

        _clientV3 = clientV3;
        _logger = logger;
        _cache = cache;
        _issueTypeService = issueTypeService;
        _componentService = componentService;
        _versionService = versionService;
    }

    public async IAsyncEnumerable<Project> GetProjectsAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        if (!_cache.Projects.Any())
        {
            _logger.LogInformation("Getting all projects");

            var models = PageExpander.ExpandAsync(
                (startAt, maxResults, t) => _clientV3.SearchProjectsAsync(startAt, maxResults, null, null, null, null, null, null, null, ProjectExpand, null, null, null, t),
                page => page.Values,
                page => page.IsLast,
                MaxProjectsResults,
                token);

            await foreach (var model in models.WithCancellation(token).ConfigureAwait(false))
            {
                _cache.Projects.TryAdd(CreateProject(model));
            }
        }

        var values = _cache.Projects.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public async Task<Project> GetProjectAsync(string projectKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        _logger.LogInformation("Getting project {ProjectKey}", projectKey);

        var model = await _clientV3.GetProjectAsync(projectKey, ProjectExpand, null, token).ConfigureAwait(false);
        return CreateProject(model);
    }

    public async Task DeleteProjectAsync(string projectKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        _logger.LogInformation("Deleting project {ProjectKey}", projectKey);

        await _clientV3.DeleteProjectAsync(projectKey, null, token).ConfigureAwait(false);

        var cached = _cache.Projects.Values.FirstOrDefault(p =>
            string.Equals(p.Key, projectKey, StringComparison.OrdinalIgnoreCase) || string.Equals(p.Id, projectKey));
        if (cached != null)
        {
            _cache.Projects.TryRemove(cached.Id);
        }
    }

    public async Task<Project> CreateProjectAsync(CreateProjectDetails project, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        _logger.LogInformation("Creating project {ProjectKey}", project.Key);

        var identifiers = await _clientV3.CreateProjectAsync(project, token).ConfigureAwait(false);

        // invalidate the cache, a partially filled one would hide the other projects
        _cache.Projects.Clear();

        return await GetProjectAsync(identifiers?.Key ?? project.Key, token).ConfigureAwait(false);
    }

    private Project CreateProject(Model.V3.Project model)
    {
        return new Project(_issueTypeService, _componentService, _versionService)
        {
            Id = model.Id,
            Name = model.Name,
            Key = model.Key,
            Category = model.ProjectCategory,
            LeadUser = model.Lead,
            Url = Uri.TryCreate(model.Url, UriKind.Absolute, out var url) ? url : null,
            AvatarUrls = model.AvatarUrls
        };
    }
}
