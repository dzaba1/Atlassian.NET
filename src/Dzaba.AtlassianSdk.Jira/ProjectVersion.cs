using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A version associated with a project
/// </summary>
public class ProjectVersion : JiraNamedEntity
{
    private readonly IJira _jira;
    private Model.V3.Version _remoteVersion;

    /// <summary>
    /// Creates a new instance of a ProjectVersion.
    /// </summary>
    /// <param name="jira">The jira instance.</param>
    /// <param name="remoteVersion">The remote version.</param>
    public ProjectVersion(IJira jira, Model.V3.Version remoteVersion)
        : base(remoteVersion.Id, remoteVersion.Name)
    {
        if (jira == null)
        {
            throw new ArgumentNullException("jira");
        }

        _jira = jira;
        _remoteVersion = remoteVersion;
    }

    internal Model.V3.Version RemoteVersion
    {
        get
        {
            return _remoteVersion;
        }
    }

    /// <summary>
    /// Gets the project key associated with this version.
    /// </summary>
    public string ProjectKey
    {
        get
        {
            return _remoteVersion.Project;
        }
    }

    /// <summary>
    /// Whether this version has been archived
    /// </summary>
    public bool IsArchived
    {
        get
        {
            return _remoteVersion.Archived;
        }
        set
        {
            _remoteVersion.Archived = value;
        }
    }

    /// <summary>
    /// Whether this version has been released
    /// </summary>
    public bool IsReleased
    {
        get
        {
            return _remoteVersion.Released;
        }
        set
        {
            _remoteVersion.Released = value;
        }
    }

    /// <summary>
    /// The start date for this version
    /// </summary>
    public DateTimeOffset StartDate
    {
        get
        {
            return _remoteVersion.StartDate;
        }
        set
        {
            _remoteVersion.StartDate = value;
        }
    }

    /// <summary>
    /// The released date for this version (null if not yet released)
    /// </summary>
    public DateTimeOffset ReleasedDate
    {
        get
        {
            return _remoteVersion.ReleaseDate;
        }
        set
        {
            _remoteVersion.ReleaseDate = value;
        }
    }

    /// <summary>
    /// The release description for this version (null if not available)
    /// </summary>
    public string Description
    {
        get
        {
            return _remoteVersion.Description;
        }
        set
        {
            _remoteVersion.Description = value;
        }
    }

    /// <summary>
    /// Save field changes to the server.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        var version = await _jira.Versions.UpdateVersionAsync(this, token).ConfigureAwait(false);
        _remoteVersion = version.RemoteVersion;
    }
}
