namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents a remote constant within JIRA. Abstracts the IssueType, Priority and Status used on issues.
/// </summary>
public class JiraNamedConstant : JiraNamedEntity
{
    /// <summary>
    /// Creates an instance of the JiraNamedConstant with the given id and name.
    /// </summary>
    public JiraNamedConstant(string id, string description, string iconUrl, string name = null)
        : base(id, name)
    {
        Description = description;
        IconUrl = iconUrl;
    }

    /// <summary>
    /// Description of the entity.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Url to the icon of this entity.
    /// </summary>
    public string IconUrl { get; private set; }
}
