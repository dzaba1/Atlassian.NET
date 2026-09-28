using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dzaba.AtlassianSdk.Jira.Http;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Model.V3;

public partial class Client : IClient
{
    // Shared across all instances so the underlying connection pool is reused instead of
    // being re-created (and exhausted) per Jira client instance.
    private static readonly HttpMessageHandler _sharedHandler = new HttpClientHandler();
    private static readonly HttpClient _sharedHttpClient = new HttpClient(_sharedHandler, disposeHandler: false);

    /// <summary>
    /// Creates a new instance of the V3 client using the shared, static <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="baseUrl">Url to the JIRA server.</param>
    /// <param name="logger"></param>
    /// <param name="authenticator">Authenticator used to authenticate every outgoing request, if any.</param>
    public Client(string baseUrl, ILogger<Client> logger = null, IHttpRequestAuthenticator authenticator = null)
        : this(CreateHttpClient(logger, authenticator))
    {
        BaseUrl = baseUrl;
    }

    private static HttpClient CreateHttpClient(ILogger<Client> logger, IHttpRequestAuthenticator authenticator)
    {
        // Wraps the shared handler rather than the shared HttpClient, so logging/authentication are
        // per-instance while the underlying connection pool is still reused.
        HttpMessageHandler handler = _sharedHandler;

        if (logger != null)
        {
            handler = new HttpLoggingHandler(logger)
            {
                InnerHandler = handler
            };
        }

        if (authenticator != null)
        {
            handler = new AuthenticatingHttpMessageHandler(authenticator)
            {
                InnerHandler = handler
            };
        }

        if (ReferenceEquals(handler, _sharedHandler))
        {
            return _sharedHttpClient;
        }

        return new HttpClient(handler, disposeHandler: false);
    }

    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        settings.PropertyNameCaseInsensitive = true;
    }
}
