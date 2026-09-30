using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the users of jira.
/// </summary>
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
    private readonly ILogger<JiraUserService> _logger;

    public JiraUserService(IClient clientV3, ILogger<JiraUserService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async Task<User> GetUserAsync(string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        _logger.LogInformation("Getting user {User}", usernameOrAccountId);

        return await _clientV3.GetUserAsync(usernameOrAccountId, null, null, null, token).ConfigureAwait(false);
    }

    public async Task DeleteUserAsync(string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        _logger.LogInformation("Deleting user {User}", usernameOrAccountId);

        await _clientV3.RemoveUserAsync(usernameOrAccountId, null, null, token).ConfigureAwait(false);
    }

    public async Task<User> CreateUserAsync(NewUserDetails newUser, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(newUser);

        _logger.LogInformation("Creating user {Email}", newUser.EmailAddress);

        return await _clientV3.CreateUserAsync(newUser, token).ConfigureAwait(false);
    }

    public async Task<User> GetMyselfAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Getting the current user");

        return await _clientV3.GetCurrentUserAsync(null, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<User> SearchUsersAsync(string query, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        _logger.LogInformation("Searching users with query {Query}", query);

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

        _logger.LogInformation("Searching users assignable to issue {IssueKey} matching {Username}", issueKey, username);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.FindAssignableUsersAsync(username, null, null, null, null, issueKey, null, (int)startAt, maxResults, null, null, null, null, t),
            MaxSearchResults,
            token);
    }

    public IAsyncEnumerable<User> SearchAssignableUsersForProjectAsync(string username, string projectKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(projectKey);

        _logger.LogInformation("Searching users assignable to project {ProjectKey} matching {Username}", projectKey, username);

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

        _logger.LogInformation("Searching users assignable to projects {ProjectKeys} matching {Username}", joinedProjectKeys, username);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.FindBulkAssignableUsersAsync(username, null, null, joinedProjectKeys, (int)startAt, maxResults, t),
            MaxSearchResults,
            token);
    }
}