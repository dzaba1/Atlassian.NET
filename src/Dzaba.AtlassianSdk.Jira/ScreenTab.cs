namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A screen tab.
/// </summary>
public class ScreenTab : JiraNamedEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenTab"/> class.
    /// </summary>
    /// <param name="remoteScreenTab">The remote screen tab.</param>
    public ScreenTab(RemoteScreenTab remoteScreenTab)
        : base(remoteScreenTab)
    {
    }
}
