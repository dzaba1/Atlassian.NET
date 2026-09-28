using System.Collections.Generic;
using System.Threading;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
public interface IRemoteIssueFieldProvider
{
    /// <summary>
    /// Gets the remote field values that should be sent to JIRA to persist changes made to this field.
    /// </summary>
    /// <param name="token">A token to cancel the operation.</param>
    IAsyncEnumerable<RemoteFieldValue> GetRemoteFieldValuesAsync(CancellationToken token);
}
