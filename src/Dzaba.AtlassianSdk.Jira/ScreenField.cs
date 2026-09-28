namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A screen field.
/// </summary>
public class ScreenField : JiraNamedEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenField"/> class.
    /// </summary>
    /// <param name="remoteScreenField">The remote screen field.</param>
    public ScreenField(RemoteScreenField remoteScreenField)
        : base(remoteScreenField)
    {
        Type = remoteScreenField.type;
    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public string Type { get; }
}
