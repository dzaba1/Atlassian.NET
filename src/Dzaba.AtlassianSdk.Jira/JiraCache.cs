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

    /// <summary>
    /// Gets the cached project versions.
    /// </summary>
    public JiraEntityDictionary<ProjectVersion> Versions { get; } = new JiraEntityDictionary<ProjectVersion>();

    /// <summary>
    /// Gets the cached issue priorities.
    /// </summary>
    public JiraEntityDictionary<Priority> Priorities { get; } = new JiraEntityDictionary<Priority>();

    /// <summary>
    /// Gets the cached issue statuses.
    /// </summary>
    public JiraEntityDictionary<StatusDetails> Statuses { get; } = new JiraEntityDictionary<StatusDetails>();

    /// <summary>
    /// Gets the cached issue resolutions.
    /// </summary>
    public JiraEntityDictionary<ResolutionJsonBean> Resolutions { get; } = new JiraEntityDictionary<ResolutionJsonBean>();

    /// <summary>
    /// Gets the cached projects.
    /// </summary>
    public JiraEntityDictionary<Project> Projects { get; } = new JiraEntityDictionary<Project>();

    /// <summary>
    /// Gets the cached custom fields.
    /// </summary>
    public JiraEntityDictionary<FieldDetails> CustomFields { get; } = new JiraEntityDictionary<FieldDetails>();

    /// <summary>
    /// Gets the cached custom fields, keyed by project key.
    /// </summary>
    public ConcurrentDictionary<string, JiraEntityDictionary<FieldDetails>> ProjectCustomFields { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<FieldDetails>>();

    /// <summary>
    /// Gets the cached issue types, keyed by project key.
    /// </summary>
    public ConcurrentDictionary<string, JiraEntityDictionary<IssueTypeWithStatus>> ProjectIssueTypes { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<IssueTypeWithStatus>>();
}
