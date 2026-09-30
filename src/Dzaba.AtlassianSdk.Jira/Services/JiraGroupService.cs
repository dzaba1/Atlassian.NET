using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the user groups of jira.
/// </summary>
public interface IJiraGroupService
{
    /// <summary>
    /// Creates a new user group.
    /// </summary>
    /// <param name="groupName">Name of group to create.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task CreateGroupAsync(string groupName, CancellationToken token = default);

    /// <summary>
    /// Deletes the group specified.
    /// </summary>
    /// <param name="groupName">Name of group to delete.</param>
    /// <param name="swapGroupName">Optional group name to transfer the restrictions (comments and worklogs only) to.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task DeleteGroupAsync(string groupName, string swapGroupName = null, CancellationToken token = default);

    /// <summary>
    /// Returns users that are members of the group specified.
    /// </summary>
    /// <param name="groupName">The name of group to return users for.</param>
    /// <param name="includeInactiveUsers">Whether to include inactive users.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<UserDetails> GetUsersAsync(string groupName, bool includeInactiveUsers = false, CancellationToken token = default);

    /// <summary>
    /// Adds a user to a the group specified.
    /// </summary>
    /// <param name="groupName">Name of group to add the user to.</param>
    /// <param name="usernameOrAccountId">User name or account id of user to add.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task AddUserAsync(string groupName, string usernameOrAccountId, CancellationToken token = default);

    /// <summary>
    /// Removes a user from the group specified.
    /// </summary>
    /// <param name="groupName">Name of the group to remove the user from.</param>
    /// <param name="usernameOrAccountId">Username or account id of user to remove.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    Task RemoveUserAsync(string groupName, string usernameOrAccountId, CancellationToken token = default);
}

internal sealed class JiraGroupService : IJiraGroupService
{
    private const int MaxGroupMembers = 50;

    private readonly IClient _clientV3;
    private readonly ILogger<JiraGroupService> _logger;

    public JiraGroupService(IClient clientV3, ILogger<JiraGroupService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);

        _clientV3 = clientV3;
        _logger = logger;
    }

    public async Task CreateGroupAsync(string groupName, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);

        _logger.LogInformation("Creating group {Group}", groupName);

        var body = new AddGroupBean { Name = groupName };
        await _clientV3.CreateGroupAsync(body, token).ConfigureAwait(false);
    }

    public async Task DeleteGroupAsync(string groupName, string swapGroupName = null, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);

        _logger.LogInformation("Deleting group {Group} with swap group {SwapGroup}", groupName, swapGroupName);

        var swapGroup = string.IsNullOrEmpty(swapGroupName) ? null : swapGroupName;
        await _clientV3.RemoveGroupAsync(groupName, null, swapGroup, null, token).ConfigureAwait(false);
    }

    public IAsyncEnumerable<UserDetails> GetUsersAsync(string groupName, bool includeInactiveUsers = false, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);

        _logger.LogInformation("Getting users of group {Group}, include inactive: {IncludeInactive}", groupName, includeInactiveUsers);

        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetUsersFromGroupAsync(groupName, null, includeInactiveUsers, startAt, maxResults, t),
            page => page.Values,
            page => page.IsLast,
            MaxGroupMembers,
            token);
    }

    public async Task AddUserAsync(string groupName, string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        _logger.LogInformation("Adding user {User} to group {Group}", usernameOrAccountId, groupName);

        var body = new UpdateUserToGroupBean { AccountId = usernameOrAccountId };
        await _clientV3.AddUserToGroupAsync(groupName, null, body, token).ConfigureAwait(false);
    }

    public async Task RemoveUserAsync(string groupName, string usernameOrAccountId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);
        ArgumentException.ThrowIfNullOrEmpty(usernameOrAccountId);

        _logger.LogInformation("Removing user {User} from group {Group}", usernameOrAccountId, groupName);

        await _clientV3.RemoveUserFromGroupAsync(groupName, null, null, usernameOrAccountId, token).ConfigureAwait(false);
    }
}