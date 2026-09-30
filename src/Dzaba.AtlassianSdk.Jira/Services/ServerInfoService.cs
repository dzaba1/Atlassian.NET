using System;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the Jira server info.
/// </summary>
public interface IServerInfoService
{
    /// <summary>
    /// Gets the server information.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The server information.</returns>
    Task<ServerInformation> GetServerInfoAsync(CancellationToken token = default);
}

internal sealed class ServerInfoService : IServerInfoService
{
    private readonly IClient _clientV3;
    private readonly ILogger<ServerInfoService> _logger;

    public ServerInfoService(IClient clientV3, ILogger<ServerInfoService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async Task<ServerInformation> GetServerInfoAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Getting the server info");

        return await _clientV3.GetServerInfoAsync(token).ConfigureAwait(false);
    }
}
