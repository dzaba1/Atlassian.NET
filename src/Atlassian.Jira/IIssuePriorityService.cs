using System.Collections.Generic;
using System.Threading;

namespace Atlassian.Jira;

/// <summary>
/// Represents the operations on the issue priorities of jira.
/// </summary>
public interface IIssuePriorityService
{
    /// <summary>
    /// Returns all the issue priorities within JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<IssuePriority> GetPrioritiesAsync(CancellationToken token = default);
}
