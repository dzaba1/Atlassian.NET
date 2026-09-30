using System;
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
}

internal sealed class JiraUserService : IJiraUserService
{
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
}