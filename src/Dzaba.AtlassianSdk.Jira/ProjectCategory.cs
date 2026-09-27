using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents a project category in Jira.
/// </summary>
public class ProjectCategory : JiraNamedResource
{
    public ProjectCategory(Model.V3.ProjectCategory remoteProjectCategory, string description)
        : base(remoteProjectCategory.Id, remoteProjectCategory.Name, remoteProjectCategory.Self)
    {
        Description = description;
    }

    /// <summary>
    /// Description of the category.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; private set; }
}
