using System;

namespace Atlassian.Jira;

/// <summary>
/// The activation status of a JIRA user.
/// </summary>
[Flags]
public enum JiraUserStatus
{
    /// <summary>
    /// The user account is active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The user account is inactive.
    /// </summary>
    Inactive = 2
}
