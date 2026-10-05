using System;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model;
using Dzaba.AtlassianSdk.Jira.Services;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A version of a JIRA project.
/// </summary>
public class ProjectVersion : IJiraEntity
{
    private readonly IProjectVersionService _versionService;

    /// <summary>
    /// Creates a project version wrapping the given model.
    /// </summary>
    /// <param name="model">The version model.</param>
    /// <param name="versionService">Service used to save the changes.</param>
    public ProjectVersion(Model.V3.Version model,
        IProjectVersionService versionService)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(versionService);

        Model = model;
        _versionService = versionService;
    }

    /// <summary>
    /// The underlying version model. Change its properties and call <see cref="SaveChangesAsync"/> to send them to the server.
    /// </summary>
    public Model.V3.Version Model { get; private set; }

    /// <summary>
    /// Id of the version.
    /// </summary>
    public string Id => Model.Id;

    /// <summary>
    /// Saves the changes made to the version to the server and refreshes it with the values returned by the server.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        var updated = await _versionService.UpdateVersionAsync(Model, token).ConfigureAwait(false);
        Model = updated.Model;
    }
}