using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira.Http;

/// <summary>
/// Applies an <see cref="IHttpRequestAuthenticator"/> to every outgoing request.
/// </summary>
internal sealed class AuthenticatingHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpRequestAuthenticator _authenticator;

    public AuthenticatingHttpMessageHandler(IHttpRequestAuthenticator authenticator)
    {
        _authenticator = authenticator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _authenticator.AuthenticateAsync(request, cancellationToken).ConfigureAwait(false);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
