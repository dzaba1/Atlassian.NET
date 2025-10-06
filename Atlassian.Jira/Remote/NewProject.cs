using Newtonsoft.Json;

namespace Atlassian.Jira.Remote;

public sealed class NewProject
{
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("projectTemplateKey")]
    public string TemplateKey { get; set; }

    [JsonProperty("projectTypeKey")]
    public string TypeKey { get; set; }

    [JsonProperty("leadAccountId")]
    public string LeadAccountId { get; set; }
}
