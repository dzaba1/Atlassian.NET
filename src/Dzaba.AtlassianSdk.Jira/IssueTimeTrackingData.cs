using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

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
    [JsonPropertyName("originalEstimate")]
    public string OriginalEstimate { get; private set; }

    /// <summary>
    /// Gets the original time estimate, in seconds.
    /// </summary>
    [JsonPropertyName("originalEstimateSeconds")]
    public long? OriginalEstimateInSeconds { get; private set; }

    /// <summary>
    /// Gets the remaining time estimate, in JIRA duration format (e.g. "1d 2h").
    /// </summary>
    [JsonPropertyName("remainingEstimate")]
    public string RemainingEstimate { get; private set; }

    /// <summary>
    /// Gets the remaining time estimate, in seconds.
    /// </summary>
    [JsonPropertyName("remainingEstimateSeconds")]
    public long? RemainingEstimateInSeconds { get; private set; }

    /// <summary>
    /// Gets the time spent, in JIRA duration format (e.g. "1d 2h").
    /// </summary>
    [JsonPropertyName("timeSpent")]
    public string TimeSpent { get; private set; }

    /// <summary>
    /// Gets the time spent, in seconds.
    /// </summary>
    [JsonPropertyName("timeSpentSeconds")]
    public long? TimeSpentInSeconds { get; private set; }
}
