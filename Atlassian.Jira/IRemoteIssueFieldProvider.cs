using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
public interface IRemoteIssueFieldProvider
{
    /// <summary>
    /// Gets the remote field values that should be sent to JIRA to persist changes made to this field.
    /// </summary>
    /// <param name="token">A token to cancel the operation.</param>
    Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken token);
}
