using System.Text.Json.Serialization;

namespace Dzaba.AtlassianSdk.Jira;

/// <remarks>
/// Class that encapsulates the necessary information to create a new jira user.
/// </remarks>
public class JiraUserCreationInfo
{
    /// <summary>
    /// Set the username
    /// </summary>
    [JsonPropertyName("name")]
    public string Username { get; set; }

    /// <summary>
    /// Set the DisplayName
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Set the email address
    /// </summary>
    [JsonPropertyName("emailAddress")]
    public string Email { get; set; }

    /// <summary>
    /// If password field is not set then password will be randomly generated.
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; set; }

    /// <summary>
    /// Set to true to have the user notified by email upon account creation. False to prevent notification.
    /// </summary>
    [JsonPropertyName("notification")]
    public bool Notification { get; set; }

    /// <summary>
    /// Returns the username of the user to create.
    /// </summary>
    public override string ToString()
    {
        return Username;
    }
}
