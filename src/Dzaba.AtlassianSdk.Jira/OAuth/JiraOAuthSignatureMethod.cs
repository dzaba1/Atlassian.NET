namespace Dzaba.AtlassianSdk.Jira.OAuth;

/// <summary>
/// Possible values for OAuth signature method.
/// </summary>
public enum JiraOAuthSignatureMethod
{
    /// <summary>
    /// HMAC-SHA1 signature method.
    /// </summary>
    HmacSha1,

    /// <summary>
    /// HMAC-SHA256 signature method.
    /// </summary>
    HmacSha256,

    /// <summary>
    /// Plain text signature method.
    /// </summary>
    PlainText,

    /// <summary>
    /// RSA-SHA1 signature method.
    /// </summary>
    RsaSha1
}
