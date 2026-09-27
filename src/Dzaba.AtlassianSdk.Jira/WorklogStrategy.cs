namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// The worklog time remaining strategy
/// </summary>
public enum WorklogStrategy
{
    /// <summary>
    /// Automatically adjusts the remaining estimate based on the time logged.
    /// </summary>
    AutoAdjustRemainingEstimate,

    /// <summary>
    /// Leaves the remaining estimate unchanged.
    /// </summary>
    RetainRemainingEstimate,

    /// <summary>
    /// Sets the remaining estimate to a new, explicitly provided value.
    /// </summary>
    NewRemainingEstimate
}
