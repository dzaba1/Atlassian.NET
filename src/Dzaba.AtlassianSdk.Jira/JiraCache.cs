using System.Collections.Concurrent;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Cache for frequently retrieved server items from JIRA.
/// </summary>
public class JiraCache
{
    /// <summary>
    /// Gets or sets the cached currently authenticated user.
    /// </summary>
    public User CurrentUser { get; set; }

    /// <summary>
    /// Gets the cached issue types.
    /// </summary>
    public JiraEntityDictionary<IssueTypeDetails> IssueTypes { get; } = new JiraEntityDictionary<IssueTypeDetails>();

    /// <summary>
    /// Gets the cached project components.
    /// </summary>
    public JiraEntityDictionary<ProjectComponent> Components { get; } = new JiraEntityDictionary<ProjectComponent>();
}
