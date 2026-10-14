using System;
using Dzaba.AtlassianSdk.Jira.Http;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Dzaba.AtlassianSdk.Jira.OAuth;

namespace Dzaba.AtlassianSdk.Jira.Model;

internal interface IClientFactory
{
    IClient CreateRestClientV3(Uri url, string username = null, string password = null, JiraRestClientSettings settings = null);
    IClient CreateOAuthRestClient(
        Uri url,
        string consumerKey,
        string consumerSecret,
        string oAuthAccessToken,
        string oAuthTokenSecret,
        JiraOAuthSignatureMethod oAuthSignatureMethod = JiraOAuthSignatureMethod.RsaSha1,
        JiraRestClientSettings settings = null);
}

internal sealed class ClientFactory : IClientFactory
{
    public IClient CreateRestClientV3(Uri url, string username = null, string password = null, JiraRestClientSettings settings = null)
    {
        ArgumentNullException.ThrowIfNull(url);

        // Anonymous access when no username is given, same as the legacy SDK.
        var authenticator = !string.IsNullOrEmpty(username)
            ? new BasicHttpRequestAuthenticator(username, password)
            : null;

        return CreateClient(url, settings, authenticator);
    }

    public IClient CreateOAuthRestClient(
        Uri url,
        string consumerKey,
        string consumerSecret,
        string oAuthAccessToken,
        string oAuthTokenSecret,
        JiraOAuthSignatureMethod oAuthSignatureMethod = JiraOAuthSignatureMethod.RsaSha1,
        JiraRestClientSettings settings = null)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfNullOrEmpty(consumerKey);
        ArgumentException.ThrowIfNullOrEmpty(consumerSecret);
        ArgumentException.ThrowIfNullOrEmpty(oAuthAccessToken);
        ArgumentException.ThrowIfNullOrEmpty(oAuthTokenSecret);

        var authenticator = new OAuth1HttpRequestAuthenticator(
            consumerKey,
            consumerSecret,
            oAuthAccessToken,
            oAuthTokenSecret,
            oAuthSignatureMethod);

        return CreateClient(url, settings, authenticator);
    }

    private static Client CreateClient(Uri url, JiraRestClientSettings settings, IHttpRequestAuthenticator authenticator)
    {
        settings ??= new JiraRestClientSettings();
        return new Client(url.ToString(), settings.GetLogger<Client>(), authenticator);
    }
}
