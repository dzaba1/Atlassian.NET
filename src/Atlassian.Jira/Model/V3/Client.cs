using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atlassian.Jira.Http;

namespace Atlassian.Jira.Model.V3;

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
    /// <param name="settings">Settings used to configure the client, e.g. request/response logging.</param>
    public Client(string baseUrl, JiraRestClientSettings settings = null)
        : this(CreateHttpClient(settings))
    {
        BaseUrl = baseUrl;
    }

    private static HttpClient CreateHttpClient(JiraRestClientSettings settings)
    {
        var logger = settings?.GetLogger<Client>();
        if (logger == null)
        {
            return _sharedHttpClient;
        }

        // Wraps the shared handler rather than the shared HttpClient, so logging is
        // per-instance while the underlying connection pool is still reused.
        var loggingHandler = new HttpLoggingHandler(logger)
        {
            InnerHandler = _sharedHandler
        };

        return new HttpClient(loggingHandler, disposeHandler: false);
    }

    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        settings.PropertyNameCaseInsensitive = true;
    }
}
