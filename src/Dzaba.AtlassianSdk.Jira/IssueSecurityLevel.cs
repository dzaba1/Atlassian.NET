using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents the security level that can be set on an issue.
/// </summary>
public class IssueSecurityLevel : JiraNamedResource
{
    /// <summary>
    /// Description of this security level.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; private set; }
}
