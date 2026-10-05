using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the Jira screens.
/// </summary>
public interface IScreenService
{
    /// <summary>
    /// Gets the screen available fields.
    /// </summary>
    /// <param name="screenId">The screen identifier.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The available fields for the given screen.</returns>
    /// <remarks>An available field is a field not yet added to a screen.</remarks>
    IAsyncEnumerable<ScreenableField> GetScreenAvailableFieldsAsync(long screenId, CancellationToken token = default);

    /// <summary>
    /// Gets the screen tabs.
    /// </summary>
    /// <param name="screenId">The screen identifier.</param>
    /// <param name="projectKey">The project key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The tabs of the given screen.</returns>
    IAsyncEnumerable<ScreenableTab> GetScreenTabsAsync(long screenId, string projectKey = null, CancellationToken token = default);

    /// <summary>
    /// Gets the screen tab fields.
    /// </summary>
    /// <param name="screenId">The screen identifier.</param>
    /// <param name="tabId">The tab identifier.</param>
    /// <param name="projectKey">The project key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The fields of the given screen tab.</returns>
    IAsyncEnumerable<ScreenableField> GetScreenTabFieldsAsync(long screenId, long tabId, string projectKey = null, CancellationToken token = default);
}

internal sealed class ScreenService : IScreenService
{
    private readonly IClient _clientV3;
    private readonly ILogger<ScreenService> _logger;

    public ScreenService(IClient clientV3, ILogger<ScreenService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    // The screen endpoints below return a bare, non paged collection (no startAt/maxResults in the contract),
    // so there is nothing to expand with PageExpander.
    public async IAsyncEnumerable<ScreenableField> GetScreenAvailableFieldsAsync(long screenId, [EnumeratorCancellation] CancellationToken token = default)
    {
        _logger.LogInformation("Getting available fields of screen {ScreenId}", screenId);

        var fields = await _clientV3.GetAvailableScreenFieldsAsync(screenId, token).ConfigureAwait(false);
        if (fields == null)
        {
            yield break;
        }

        foreach (var field in fields)
        {
            yield return field;
        }
    }

    public async IAsyncEnumerable<ScreenableTab> GetScreenTabsAsync(long screenId, string projectKey = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        _logger.LogInformation("Getting tabs of screen {ScreenId} for project {ProjectKey}", screenId, projectKey);

        var tabs = await _clientV3.GetAllScreenTabsAsync(screenId, NormalizeProjectKey(projectKey), token).ConfigureAwait(false);
        if (tabs == null)
        {
            yield break;
        }

        foreach (var tab in tabs)
        {
            yield return tab;
        }
    }

    public async IAsyncEnumerable<ScreenableField> GetScreenTabFieldsAsync(long screenId, long tabId, string projectKey = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        _logger.LogInformation("Getting fields of tab {TabId} of screen {ScreenId} for project {ProjectKey}", tabId, screenId, projectKey);

        var fields = await _clientV3.GetAllScreenTabFieldsAsync(screenId, tabId, NormalizeProjectKey(projectKey), token).ConfigureAwait(false);
        if (fields == null)
        {
            yield break;
        }

        foreach (var field in fields)
        {
            yield return field;
        }
    }

    private static string NormalizeProjectKey(string projectKey)
    {
        return string.IsNullOrWhiteSpace(projectKey) ? null : projectKey;
    }
}