using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Class that encapsulates the necessary information to create a new project component.
/// </summary>
public class ProjectComponentCreationInfo
{
    /// <summary>
    /// Creates a new instance of ProjectComponentCreationInfo.
    /// </summary>
    /// <param name="name">The name of the project component.</param>
    public ProjectComponentCreationInfo(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Name of the project component.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description of the project component.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Key of the project to associate with this component.
    /// </summary>
    [JsonPropertyName("project")]
    public string ProjectKey { get; set; }
}
