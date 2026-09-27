using System.Collections.Concurrent;

namespace Atlassian.Jira;

/// <summary>
/// Cache for frequently retrieved server items from JIRA.
/// </summary>
public class JiraCache
{
    /// <summary>
    /// Gets or sets the cached currently authenticated user.
    /// </summary>
    public JiraUser CurrentUser { get; set; }

    /// <summary>
    /// Gets the cached issue types.
    /// </summary>
    public JiraEntityDictionary<IssueType> IssueTypes { get; } = new JiraEntityDictionary<IssueType>();

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
    public JiraEntityDictionary<IssuePriority> Priorities { get; } = new JiraEntityDictionary<IssuePriority>();

    /// <summary>
    /// Gets the cached issue statuses.
    /// </summary>
    public JiraEntityDictionary<IssueStatus> Statuses { get; } = new JiraEntityDictionary<IssueStatus>();

    /// <summary>
    /// Gets the cached issue resolutions.
    /// </summary>
    public JiraEntityDictionary<IssueResolution> Resolutions { get; } = new JiraEntityDictionary<IssueResolution>();

    /// <summary>
    /// Gets the cached projects.
    /// </summary>
    public JiraEntityDictionary<Project> Projects { get; } = new JiraEntityDictionary<Project>();

    /// <summary>
    /// Gets the cached custom fields.
    /// </summary>
    public JiraEntityDictionary<CustomField> CustomFields { get; } = new JiraEntityDictionary<CustomField>();

    /// <summary>
    /// Gets the cached issue link types.
    /// </summary>
    public JiraEntityDictionary<IssueLinkType> LinkTypes { get; } = new JiraEntityDictionary<IssueLinkType>();

    /// <summary>
    /// Gets the cached custom fields, keyed by project key.
    /// </summary>
    public ConcurrentDictionary<string, JiraEntityDictionary<CustomField>> ProjectCustomFields { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<CustomField>>();

    /// <summary>
    /// Gets the cached issue types, keyed by project key.
    /// </summary>
    public ConcurrentDictionary<string, JiraEntityDictionary<IssueType>> ProjectIssueTypes { get; } = new ConcurrentDictionary<string, JiraEntityDictionary<IssueType>>();
}
