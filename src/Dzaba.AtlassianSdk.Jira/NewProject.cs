using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents the data required to create a new project in JIRA.
/// </summary>
public sealed class NewProject
{
    /// <summary>
    /// Gets or sets the key of the new project.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the name of the new project.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the new project.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the key of the project template to create the project from.
    /// </summary>
    [JsonPropertyName("projectTemplateKey")]
    public string TemplateKey { get; set; }

    /// <summary>
    /// Gets or sets the key of the project type (e.g. "business", "software").
    /// </summary>
    [JsonPropertyName("projectTypeKey")]
    public string TypeKey { get; set; }

    /// <summary>
    /// Gets or sets the account identifier of the project lead.
    /// </summary>
    [JsonPropertyName("leadAccountId")]
    public string LeadAccountId { get; set; }
}
