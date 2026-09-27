using System.Collections.Generic;
using System.Threading;

namespace Atlassian.Jira;

/// <summary>
/// Represents the operations on the issue types of jira.
/// Maps to https://docs.atlassian.com/jira/REST/latest/#api/2/issuetype.
/// </summary>
public interface IIssueTypeService
{
    /// <summary>
    /// Returns all the issue types within JIRA.
    /// </summary>
    IAsyncEnumerable<IssueType> GetIssueTypesAsync(CancellationToken token = default);

    /// <summary>
    /// Returns the issue types within JIRA for the project specified.
    /// </summary>
    IAsyncEnumerable<IssueType> GetIssueTypesForProjectAsync(string projectKey, CancellationToken token = default);

}
