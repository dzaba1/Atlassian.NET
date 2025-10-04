using System.Collections.Generic;
using System.Threading;

namespace Atlassian.Jira;

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
    IAsyncEnumerable<ScreenField> GetScreenAvailableFieldsAsync(string screenId, CancellationToken token = default);

    /// <summary>
    /// Gets the screen tabs.
    /// </summary>
    /// <param name="screenId">The screen identifier.</param>
    /// <param name="projectKey">The project key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The tabs of the given screen.</returns>
    IAsyncEnumerable<ScreenTab> GetScreenTabsAsync(string screenId, string projectKey = null, CancellationToken token = default);

    /// <summary>
    /// Gets the screen tab fields.
    /// </summary>
    /// <param name="screenId">The screen identifier.</param>
    /// <param name="tabId">The tab identifier.</param>
    /// <param name="projectKey">The project key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The fields of the given screen tab.</returns>
    IAsyncEnumerable<ScreenField> GetScreenTabFieldsAsync(string screenId, string tabId, string projectKey = null, CancellationToken token = default);
}
