using Newtonsoft.Json;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Represents the data required to create a new project in JIRA.
/// </summary>
public sealed class NewProject
{
    /// <summary>
    /// Gets or sets the key of the new project.
    /// </summary>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the name of the new project.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the new project.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the key of the project template to create the project from.
    /// </summary>
    [JsonProperty("projectTemplateKey")]
    public string TemplateKey { get; set; }

    /// <summary>
    /// Gets or sets the key of the project type (e.g. "business", "software").
    /// </summary>
    [JsonProperty("projectTypeKey")]
    public string TypeKey { get; set; }

    /// <summary>
    /// Gets or sets the account identifier of the project lead.
    /// </summary>
    [JsonProperty("leadAccountId")]
    public string LeadAccountId { get; set; }
}
