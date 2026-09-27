using Newtonsoft.Json;

namespace Atlassian.Jira;

/// <summary>
/// Urls for the different renditions of an avatar.
/// </summary>
public class AvatarUrls
{
    /// <summary>
    /// Gets or sets the URL of the 16x16 pixel avatar rendition.
    /// </summary>
    [JsonProperty("16x16")]
    public string XSmall { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 24x24 pixel avatar rendition.
    /// </summary>
    [JsonProperty("24x24")]
    public string Small { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 32x32 pixel avatar rendition.
    /// </summary>
    [JsonProperty("32x32")]
    public string Medium { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 48x48 pixel avatar rendition.
    /// </summary>
    [JsonProperty("48x48")]
    public string Large { get; set; }
}
