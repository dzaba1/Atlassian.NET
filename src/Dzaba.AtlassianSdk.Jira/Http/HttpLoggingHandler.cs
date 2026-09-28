using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Http;

internal sealed class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public HttpLoggingHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] Request Url: {Url}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogInformation("[{Method}] Request Data: {Body}", request.Method, body);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _logger.LogInformation("[{Method}] Response for Url: {Url}" + Environment.NewLine + "{Content}", request.Method, request.RequestUri, content);

        return response;
    }
}
