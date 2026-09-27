using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents the log of the change done to an issue as recorded by JIRA.
/// </summary>
public class IssueChangeLog : IJiraEntity
{
    /// <summary>
    /// Identifier of this change log.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; private set; }

    /// <summary>
    /// User that performed the change.
    /// </summary>
    [JsonPropertyName("author")]
    public JiraUser Author { get; private set; }

    /// <summary>
    /// Date that the change was performed.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime CreatedDate { get; private set; }

    /// <summary>
    /// List of items that were changed.
    /// </summary>
    [JsonPropertyName("items")]
    public IEnumerable<IssueChangeLogItem> Items { get; private set; }
}
