using System;
using Atlassian.Jira.Model.V3;
using Newtonsoft.Json;

namespace Atlassian.Jira;

/// <summary>
/// Urls for the different renditions of an avatar.
/// </summary>
public class AvatarUrls
{
    public AvatarUrls()
    {
        
    }

    public AvatarUrls(AvatarUrlsBean model)
    {
        XSmall = model._16x16;
        Small = model._24x24;
        Medium = model._32x32;
        Large = model._48x48;
    }

    /// <summary>
    /// Gets or sets the URL of the 16x16 pixel avatar rendition.
    /// </summary>
    [JsonProperty("16x16")]
    public Uri XSmall { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 24x24 pixel avatar rendition.
    /// </summary>
    [JsonProperty("24x24")]
    public Uri Small { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 32x32 pixel avatar rendition.
    /// </summary>
    [JsonProperty("32x32")]
    public Uri Medium { get; set; }

    /// <summary>
    /// Gets or sets the URL of the 48x48 pixel avatar rendition.
    /// </summary>
    [JsonProperty("48x48")]
    public Uri Large { get; set; }
}
