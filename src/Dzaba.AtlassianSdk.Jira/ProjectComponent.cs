using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A component associated with a project
/// </summary>
public class ProjectComponent : JiraNamedEntity
{
    private readonly ComponentWithIssueCount _remoteComponent;

    /// <summary>
    /// Creates a new instance of ProjectComponent.
    /// </summary>
    /// <param name="remoteComponent">The remote component.</param>
    public ProjectComponent(ComponentWithIssueCount remoteComponent)
        : base(remoteComponent.Id, remoteComponent.Name)
    {
        _remoteComponent = remoteComponent;
    }

    internal ComponentWithIssueCount RemoteComponent
    {
        get
        {
            return _remoteComponent;
        }
    }

    /// <summary>
    /// Gets the project key associated with this component.
    /// </summary>
    public string ProjectKey
    {
        get
        {
            return _remoteComponent.Project;
        }
    }
}
