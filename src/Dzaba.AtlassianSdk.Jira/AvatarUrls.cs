using System;
using System.Text.Json.Serialization;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Urls for the different renditions of an avatar.
/// </summary>
public class AvatarUrls
{
    public AvatarUrls()
    {
        
    }

    public AvatarUrls(UserBeanAvatarUrls remoteAvatarUrls)
    {
        XSmall = remoteAvatarUrls._16x16;
        Small = remoteAvatarUrls._24x24;
        Medium = remoteAvatarUrls._32x32;
        Large = remoteAvatarUrls._48x48;
    }

    public AvatarUrls(AvatarUrlsBean remoteAvatarUrls)
    {
        XSmall = remoteAvatarUrls._16x16;
        Small = remoteAvatarUrls._24x24;
        Medium = remoteAvatarUrls._32x32;
        Large = remoteAvatarUrls._48x48;
    }

    /// <summary>
    /// Gets or sets the URL of the 16x16 pixel avatar rendition.
    /// </summary>
    [JsonPropertyName("16x16")]
    public Uri XSmall { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 24x24 pixel avatar rendition.
    /// </summary>
    [JsonPropertyName("24x24")]
    public Uri Small { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 32x32 pixel avatar rendition.
    /// </summary>
    [JsonPropertyName("32x32")]
    public Uri Medium { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 48x48 pixel avatar rendition.
    /// </summary>
    [JsonPropertyName("48x48")]
    public Uri Large { get; set; }
}
