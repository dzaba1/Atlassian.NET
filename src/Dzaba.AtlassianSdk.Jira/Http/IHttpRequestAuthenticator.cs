using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.Http;

/// <summary>
/// Applies authentication to an outgoing HTTP request.
/// </summary>
public interface IHttpRequestAuthenticator
{
    /// <summary>
    /// Adds the authentication information (e.g. a header) to the given request.
    /// </summary>
    Task AuthenticateAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
