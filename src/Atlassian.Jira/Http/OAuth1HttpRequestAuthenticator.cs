using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.OAuth;

namespace Atlassian.Jira.Http;

/// <summary>
/// Authenticates requests using the OAuth 1.0 protocol (RFC 5849), signing each request
/// with the consumer key/secret and access token/secret of a protected resource request.
/// </summary>
internal sealed class OAuth1HttpRequestAuthenticator : IHttpRequestAuthenticator
{
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly string _consumerKey;
    private readonly string _consumerSecret;
    private readonly string _accessToken;
    private readonly string _accessTokenSecret;
    private readonly JiraOAuthSignatureMethod _signatureMethod;

    public OAuth1HttpRequestAuthenticator(
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        JiraOAuthSignatureMethod signatureMethod)
    {
        _consumerKey = consumerKey;
        _consumerSecret = consumerSecret;
        _accessToken = accessToken;
        _accessTokenSecret = accessTokenSecret;
        _signatureMethod = signatureMethod;
    }

    public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;

        var oauthParameters = new List<(string Name, string Value)>
        {
            ("oauth_consumer_key", _consumerKey),
            ("oauth_nonce", Guid.NewGuid().ToString("N")),
            ("oauth_signature_method", GetSignatureMethodName(_signatureMethod)),
            ("oauth_timestamp", GetTimestamp()),
            ("oauth_token", _accessToken),
            ("oauth_version", "1.0"),
        };

        var signatureBase = BuildSignatureBase(request.Method.Method, uri, oauthParameters.Concat(GetQueryParameters(uri)));
        var signature = ComputeSignature(_signatureMethod, signatureBase, _consumerSecret, _accessTokenSecret);
        oauthParameters.Add(("oauth_signature", signature));

        request.Headers.TryAddWithoutValidation("Authorization", BuildAuthorizationHeader(oauthParameters));

        return Task.CompletedTask;
    }

    private static string GetTimestamp() =>
        ((long)(DateTime.UtcNow - Epoch).TotalSeconds).ToString(CultureInfo.InvariantCulture);

    private static string GetSignatureMethodName(JiraOAuthSignatureMethod method) => method switch
    {
        JiraOAuthSignatureMethod.HmacSha1 => "HMAC-SHA1",
        JiraOAuthSignatureMethod.HmacSha256 => "HMAC-SHA256",
        JiraOAuthSignatureMethod.PlainText => "PLAINTEXT",
        JiraOAuthSignatureMethod.RsaSha1 => "RSA-SHA1",
        _ => throw new NotSupportedException($"OAuth signature method '{method}' is not supported."),
    };

    private static IEnumerable<(string Name, string Value)> GetQueryParameters(Uri uri)
    {
        if (string.IsNullOrEmpty(uri.Query))
        {
            yield break;
        }

        foreach (var pair in uri.Query.TrimStart('?').Split('&'))
        {
            if (pair.Length == 0)
            {
                continue;
            }

            var parts = pair.Split(new[] { '=' }, 2);
            var name = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            yield return (name, value);
        }
    }

    // See https://tools.ietf.org/html/rfc5849#section-3.4.1
    private static string BuildSignatureBase(string httpMethod, Uri uri, IEnumerable<(string Name, string Value)> parameters)
    {
        var baseUri = uri.GetLeftPart(UriPartial.Path);
        var normalizedParameters = BuildNormalizedParameters(parameters);

        return $"{httpMethod.ToUpperInvariant()}&{PercentEncode(baseUri)}&{PercentEncode(normalizedParameters)}";
    }

    private static string BuildNormalizedParameters(IEnumerable<(string Name, string Value)> parameters)
    {
        var encoded = parameters
            .Select(p => (Name: PercentEncode(p.Name), Value: PercentEncode(p.Value)))
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ThenBy(p => p.Value, StringComparer.Ordinal);

        return string.Join("&", encoded.Select(p => $"{p.Name}={p.Value}"));
    }

    private static string BuildAuthorizationHeader(IEnumerable<(string Name, string Value)> parameters)
    {
        var headerParameters = parameters.Select(p => $"{PercentEncode(p.Name)}=\"{PercentEncode(p.Value)}\"");
        return "OAuth " + string.Join(",", headerParameters);
    }

    private static string ComputeSignature(JiraOAuthSignatureMethod method, string signatureBase, string consumerSecret, string tokenSecret)
    {
        var key = $"{PercentEncode(consumerSecret)}&{PercentEncode(tokenSecret)}";

        switch (method)
        {
            case JiraOAuthSignatureMethod.PlainText:
                return key;

            case JiraOAuthSignatureMethod.HmacSha1:
                using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
                {
                    return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBase)));
                }

            case JiraOAuthSignatureMethod.HmacSha256:
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
                {
                    return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBase)));
                }

            case JiraOAuthSignatureMethod.RsaSha1:
                // Kept consistent with the RSA-SHA1 implementation used for the v2 client's OAuth authenticator:
                // the consumer secret is the consumer's RSA private key, in XML format.
                using (var rsa = new RSACryptoServiceProvider { PersistKeyInCsp = false })
                {
                    rsa.FromXmlString(consumerSecret);
                    using (var sha1 = SHA1.Create())
                    {
                        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBase));
                        return Convert.ToBase64String(rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1")));
                    }
                }

            default:
                throw new NotSupportedException($"OAuth signature method '{method}' is not supported.");
        }
    }

    private static string PercentEncode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Uri.EscapeDataString already implements RFC 3986 percent-encoding for everything outside the
        // unreserved set (ALPHA / DIGIT / "-" / "." / "_" / "~"), except that it leaves !*'() unescaped;
        // OAuth 1.0 (RFC 5849 section 3.6) requires those to be encoded too.
        var escaped = new StringBuilder(Uri.EscapeDataString(value));
        escaped.Replace("!", "%21");
        escaped.Replace("*", "%2A");
        escaped.Replace("'", "%27");
        escaped.Replace("(", "%28");
        escaped.Replace(")", "%29");

        return escaped.ToString();
    }
}
