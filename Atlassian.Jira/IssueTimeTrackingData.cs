using Newtonsoft.Json;

namespace Atlassian.Jira;

/// <summary>
/// Time tracking information for an issue.
/// </summary>
public class IssueTimeTrackingData
{
    /// <summary>
    /// Creates a new instance of the IssueTimeTrackingData class.
    /// </summary>
    public IssueTimeTrackingData(string originalEstimate, string remainingEstimate = null)
    {
        OriginalEstimate = originalEstimate;
        RemainingEstimate = remainingEstimate;
    }

    /// <summary>
    /// Gets the original time estimate, in JIRA duration format (e.g. "1d 2h").
    /// </summary>
    [JsonProperty("originalEstimate")]
    public string OriginalEstimate { get; private set; }

    /// <summary>
    /// Gets the original time estimate, in seconds.
    /// </summary>
    [JsonProperty("originalEstimateSeconds")]
    public long? OriginalEstimateInSeconds { get; private set; }

    /// <summary>
    /// Gets the remaining time estimate, in JIRA duration format (e.g. "1d 2h").
    /// </summary>
    [JsonProperty("remainingEstimate")]
    public string RemainingEstimate { get; private set; }

    /// <summary>
    /// Gets the remaining time estimate, in seconds.
    /// </summary>
    [JsonProperty("remainingEstimateSeconds")]
    public long? RemainingEstimateInSeconds { get; private set; }

    /// <summary>
    /// Gets the time spent, in JIRA duration format (e.g. "1d 2h").
    /// </summary>
    [JsonProperty("timeSpent")]
    public string TimeSpent { get; private set; }

    /// <summary>
    /// Gets the time spent, in seconds.
    /// </summary>
    [JsonProperty("timeSpentSeconds")]
    public long? TimeSpentInSeconds { get; private set; }
}
