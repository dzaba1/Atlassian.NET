using Newtonsoft.Json;

namespace Atlassian.Jira;

/// <summary>
/// Represents the visibility field on a comment.
/// </summary>
public class CommentVisibility
{
    /// <summary>
    /// Create an empty comment visibility object.
    /// </summary>
    public CommentVisibility()
    {
    }

    /// <summary>
    /// Creates a comment visibility object with the given role.
    /// </summary>
    /// <param name="role">The role to apply to the visibility object.</param>
    public CommentVisibility(string role)
    {
        Type = "role";
        Value = role;
    }

    /// <summary>
    /// Gets or sets the type of visibility restriction (e.g. "role" or "group").
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the role or group the comment is restricted to.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }
}
