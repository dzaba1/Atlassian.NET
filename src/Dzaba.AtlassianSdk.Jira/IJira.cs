using Dzaba.AtlassianSdk.Jira.Services;

namespace Dzaba.AtlassianSdk.Jira;

public interface IJira
{
    /// <summary>
    /// Gets an object to interact with the projects of jira.
    /// </summary>
    IProjectService Projects { get; }

    /// <summary>
    /// Gets an object to interact with the users of jira.
    /// </summary>
    IJiraUserService Users  { get; }

    /// <summary>
    /// Gets an object to interact with the user groups of jira.
    /// </summary>
    IJiraGroupService Groups { get; }

    /// <summary>
    /// Gets an object to interact with the issue of jira.
    /// </summary>
    IIssueService Issues { get; }

    /// <summary>
    /// Gets an object to interact with the issue fields of jira.
    /// </summary>
    IIssueFieldService Fields { get; }

    /// <summary>
    /// Gets an object to interact with the issue filters of jira.
    /// </summary>
    IIssueFilterService Filters { get; }

    /// <summary>
    /// Gets an object to interact with the issue priorities of jira.
    /// </summary>
    IIssuePriorityService Priorities { get; }

    /// <summary>
    /// Gets an object to interact with the issue resolutions of jira.
    /// </summary>
    IIssueResolutionService Resolutions { get; }

    /// <summary>
    /// Gets an object to interact with the issue statuses of jira.
    /// </summary>
    IIssueStatusService Statuses { get; }

    /// <summary>
    /// Gets an object to interact with the issue link types of jira.
    /// </summary>
    IIssueLinkService Links { get; }

    /// <summary>
    /// Gets an object to interact with the issue remote links of jira.
    /// </summary>
    IIssueRemoteLinkService RemoteLinks { get; }

    /// <summary>
    /// Gets an object to interact with the issue types of jira.
    /// </summary>
    IIssueTypeService IssueTypes { get; }

    /// <summary>
    /// Gets an object to interact with the project versions of jira.
    /// </summary>
    IProjectVersionService Versions { get; }

    /// <summary>
    /// Gets an object to interact with the project components of jira.
    /// </summary>
    IProjectComponentService Components { get; }

    /// <summary>
    /// Gets an object to interact with the Jira screens.
    /// </summary>
    IScreenService Screens { get; }

    /// <summary>
    /// Gets an object to interact with the server information.
    /// </summary>
    IServerInfoService ServerInfo { get; }
}