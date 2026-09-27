using System;
using Atlassian.Jira.Model.V3;

namespace Atlassian.Jira;

/// <summary>
/// Represents a JIRA user.
/// </summary>
public class JiraUser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JiraUser"/> class based on its remote counterpart.
    /// </summary>
    /// <param name="remoteUser">The remote user.</param>
    /// <param name="userPrivacyEnabled">if set to <c>true</c> enable user privacy mode (use 'accountId' insead of 'name' for serialization).</param>
    public JiraUser(User remoteUser, bool userPrivacyEnabled = false)
    {
        AccountId = remoteUser.AccountId;
        DisplayName = remoteUser.DisplayName;
        Email = remoteUser.EmailAddress;
        IsActive = remoteUser.Active;
        Key = remoteUser.Key;
        Locale = remoteUser.Locale;
        Self = remoteUser.Self;
        Username = remoteUser.Name;
        AvatarUrls = new AvatarUrls(remoteUser.AvatarUrls);
        InternalIdentifier = userPrivacyEnabled ? remoteUser.AccountId : remoteUser.Name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JiraUser"/> class.
    /// </summary>
    internal JiraUser()
    {
    }

    /// <summary>
    /// The Atlassian account identifier for this user.
    /// </summary>
    public string AccountId { get; internal set; }

    /// <summary>
    /// The identifier for the user as defined by JIRA.
    /// </summary>
    public string Key { get; internal set; }

    /// <summary>
    /// The 'username' for the user.
    /// </summary>
    public string Username { get; internal set; }

    /// <summary>
    /// The long display name for the user.
    /// </summary>
    public string DisplayName { get; internal set; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; internal set; }

    /// <summary>
    /// Whether the user is marked as active on the server.
    /// </summary>
    public bool IsActive { get; internal set; }

    /// <summary>
    /// The locale of the User.
    /// </summary>
    public string Locale { get; internal set; }

    /// <summary>
    /// Url to access this resource.
    /// </summary>
    public Uri Self { get; internal set; }

    /// <summary>
    /// The list of the Avatar URL's for this user
    /// </summary>
    public AvatarUrls AvatarUrls { get; internal set; }

    internal string InternalIdentifier { get; set; }

    /// <summary>
    /// Returns the internal identifier used to reference this user (either the account id or username).
    /// </summary>
    public override string ToString()
    {
        return InternalIdentifier;
    }

    /// <summary>
    /// Determines whether the specified object represents the same JIRA user.
    /// </summary>
    /// <param name="other">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="JiraUser"/> with the same internal identifier; otherwise, <c>false</c>.</returns>
    public override bool Equals(object other)
    {
        var otherAsThisType = other as JiraUser;
        return otherAsThisType != null && InternalIdentifier.Equals(otherAsThisType.InternalIdentifier);
    }

    /// <summary>
    /// Returns a hash code based on the user's internal identifier.
    /// </summary>
    public override int GetHashCode()
    {
        return InternalIdentifier.GetHashCode();
    }
}
