using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira.Services;

public interface IJiraUserService
{
    /// <summary>
    /// Retrieve user specified by username.
    /// </summary>
    /// <param name="usernameOrAccountId">The username or account id of the user to get.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<User> GetUserAsync(string usernameOrAccountId, CancellationToken token = default);

    /// <summary>
    /// Deletes a user by the given username.
    /// </summary>
    /// <param name="usernameOrAccountId">User name or account id of user to delete.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task DeleteUserAsync(string usernameOrAccountId, CancellationToken token = default);

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="newUser">The information about the user to be created.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<User> CreateUserAsync(NewUserDetails newUser, CancellationToken token = default);

    /// <summary>
    /// Retrieve user currently connected.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    Task<User> GetMyselfAsync(CancellationToken token = default);

    /// <summary>
    /// Finds users with a structured query, for example "is assignee of PROJ". All pages are returned.
    /// </summary>
    /// <param name="query">The structured search query.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<User> SearchUsersAsync(string query, CancellationToken token = default);

    /// <summary>
    /// Searches assignable users for an issue.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="issueKey">The issue key.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<User> SearchAssignableUsersForIssueAsync(string username, string issueKey, CancellationToken token = default);

    /// <summary>
    /// Searches assignable users for a project.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="projectKey">The project key.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<User> SearchAssignableUsersForProjectAsync(string username, string projectKey, CancellationToken token = default);

    /// <summary>
    /// Searches the assignable users for a list of projects.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="projectKeys">The project keys.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<User> SearchAssignableUsersForProjectsAsync(string username, IEnumerable<string> projectKeys, CancellationToken token = default);
}

internal sealed class JiraUserService : IJiraUserService
{
    private const int MaxSearchResults = 50;

    private readonly IClient _clientV3;

    public JiraUserService(IClient clientV3)
    {
        ArgumentNullException.ThrowIfNull(clientV3);

        _clientV3 = clientV3;
    }

    public async Task<User> GetUserAsync(string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        return await _clientV3.GetUserAsync(usernameOrAccountId, null, null, null, token).ConfigureAwait(false);
    }

    public async Task DeleteUserAsync(string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        await _clientV3.RemoveUserAsync(usernameOrAccountId, null, null, token).ConfigureAwait(false);
    }

    public async Task<User> CreateUserAsync(NewUserDetails newUser, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(newUser);

        return await _clientV3.CreateUserAsync(newUser, token).ConfigureAwait(false);
    }

    public async Task<User> GetMyselfAsync(CancellationToken token = default)
    {
        return await _clientV3.GetCurrentUserAsync(null, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<User> SearchUsersAsync(string query, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.FindUsersByQueryAsync(query, startAt, maxResults, t),
            page => page.Values,
            page => page.IsLast,
            MaxSearchResults,
            token);
    }

    public IAsyncEnumerable<User> SearchAssignableUsersForIssueAsync(string username, string issueKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(issueKey);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.FindAssignableUsersAsync(username, null, null, null, null, issueKey, null, (int)startAt, maxResults, null, null, null, null, t),
            MaxSearchResults,
            token);
    }

    public IAsyncEnumerable<User> SearchAssignableUsersForProjectAsync(string username, string projectKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.FindAssignableUsersAsync(username, null, null, null, projectKey, null, null, (int)startAt, maxResults, null, null, null, null, t),
            MaxSearchResults,
            token);
    }

    public IAsyncEnumerable<User> SearchAssignableUsersForProjectsAsync(string username, IEnumerable<string> projectKeys, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentNullException.ThrowIfNull(projectKeys);

        var joinedProjectKeys = string.Join(",", projectKeys);
        ArgumentException.ThrowIfNullOrEmpty(joinedProjectKeys, nameof(projectKeys));

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.FindBulkAssignableUsersAsync(username, null, null, joinedProjectKeys, (int)startAt, maxResults, t),
            MaxSearchResults,
            token);
    }
}