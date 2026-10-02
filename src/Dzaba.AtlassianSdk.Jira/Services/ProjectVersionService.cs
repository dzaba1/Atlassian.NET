using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations for the project versions.
/// </summary>
public interface IProjectVersionService
{
    /// <summary>
    /// Creates a new project version.
    /// </summary>
    /// <param name="projectVersion">Information of the new project version.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<Model.V3.Version> CreateVersionAsync(Model.V3.Version projectVersion, CancellationToken token = default);

    /// <summary>
    /// Deletes a project version.
    /// </summary>
    /// <param name="versionId">Identifier of the version to delete.</param>
    /// <param name="moveFixIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
    /// <param name="moveAffectedIssuesTo">The version to set fixVersion to on issues where the deleted version is the fix version, If null then the fixVersion is removed.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task DeleteVersionAsync(string versionId, string moveFixIssuesTo = null, string moveAffectedIssuesTo = null, CancellationToken token = default);

    /// <summary>
    /// Gets the versions for a given project.
    /// </summary>
    /// <param name="projectKey">Key of the project to retrieve versions from.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<Model.V3.Version> GetVersionsAsync(string projectKey, CancellationToken token = default);

    /// <summary>
    /// Gets the version specified.
    /// </summary>
    /// <param name="versionId">Identifier of the version.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<Model.V3.Version> GetVersionAsync(string versionId, CancellationToken token = default);

    /// <summary>
    /// Updates a version and returns a new instance populated from server.
    /// </summary>
    /// <param name="version">Version to update.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<Model.V3.Version> UpdateVersionAsync(Model.V3.Version version, CancellationToken token = default);
}

internal sealed class ProjectVersionService : IProjectVersionService
{
    private const int MaxVersionsResults = 50;

    // The delete and replace endpoint has no way to omit the replacement, -1 clears the version from the issues.
    private const long RemoveVersionFromIssues = -1;

    private readonly IClient _clientV3;
    private readonly ILogger<ProjectVersionService> _logger;

    public ProjectVersionService(IClient clientV3, ILogger<ProjectVersionService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async Task<Model.V3.Version> CreateVersionAsync(Model.V3.Version projectVersion, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(projectVersion);

        _logger.LogInformation("Creating version {Version} in project {ProjectKey}", projectVersion.Name, projectVersion.Project);

        return await _clientV3.CreateVersionAsync(projectVersion, token).ConfigureAwait(false);
    }

    public async Task DeleteVersionAsync(string versionId, string moveFixIssuesTo = null, string moveAffectedIssuesTo = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionId);

        _logger.LogInformation("Deleting version {VersionId} with moving fix issues to {MoveFixIssuesTo} and affected issues to {MoveAffectedIssuesTo}",
            versionId, moveFixIssuesTo, moveAffectedIssuesTo);

        var body = new DeleteAndReplaceVersionBean
        {
            MoveFixIssuesTo = ParseReplacementVersionId(moveFixIssuesTo, nameof(moveFixIssuesTo)),
            MoveAffectedIssuesTo = ParseReplacementVersionId(moveAffectedIssuesTo, nameof(moveAffectedIssuesTo))
        };
        await _clientV3.DeleteAndReplaceVersionAsync(versionId, body, token).ConfigureAwait(false);
    }

    private static long ParseReplacementVersionId(string versionId, string paramName)
    {
        if (string.IsNullOrEmpty(versionId))
        {
            return RemoveVersionFromIssues;
        }

        if (!long.TryParse(versionId, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            throw new ArgumentException($"'{versionId}' is not a valid version identifier.", paramName);
        }

        return id;
    }

    public IAsyncEnumerable<Model.V3.Version> GetVersionsAsync(string projectKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        _logger.LogInformation("Getting versions of project {ProjectKey}", projectKey);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetProjectVersionsPaginatedAsync(projectKey, startAt, maxResults, null, null, null, null, t),
            page => page.Values,
            page => page.IsLast,
            MaxVersionsResults,
            token);
    }

    public async Task<Model.V3.Version> GetVersionAsync(string versionId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionId);

        _logger.LogInformation("Getting version {VersionId}", versionId);

        return await _clientV3.GetVersionAsync(versionId, null, token).ConfigureAwait(false);
    }

    public async Task<Model.V3.Version> UpdateVersionAsync(Model.V3.Version version, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentException.ThrowIfNullOrEmpty(version.Id, nameof(version));

        _logger.LogInformation("Updating version {VersionId}", version.Id);

        return await _clientV3.UpdateVersionAsync(version.Id, version, token).ConfigureAwait(false);
    }
}