using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.Http;

/// <summary>
/// Authenticates requests using HTTP Basic authentication.
/// </summary>
internal sealed class BasicHttpRequestAuthenticator : IHttpRequestAuthenticator
{
    private readonly string _headerValue;

    public BasicHttpRequestAuthenticator(string username, string password)
    {
        _headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
    }

    public Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _headerValue);
        return Task.CompletedTask;
    }
}
