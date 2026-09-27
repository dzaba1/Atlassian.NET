namespace Atlassian.Jira;

/// <summary>
/// Default workflow actions for a standard JIRA install.
/// </summary>
public static class WorkflowActions
{
    /// <summary>
    /// The name of the default action that resolves an issue.
    /// </summary>
    public const string Resolve = "Resolve Issue";

    /// <summary>
    /// The name of the default action that closes an issue.
    /// </summary>
    public const string Close = "Close Issue";

    /// <summary>
    /// The name of the default action that starts progress on an issue.
    /// </summary>
    public const string StartProgress = "Start Progress";
}
