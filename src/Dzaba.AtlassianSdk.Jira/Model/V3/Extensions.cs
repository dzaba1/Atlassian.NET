using System;

namespace Dzaba.AtlassianSdk.Jira.Model.V3;

public static class Extensions
{
    public static string GetInternalIdentifier(this User user, bool userPrivacyEnabled)
    {
        ArgumentNullException.ThrowIfNull(user);

        return userPrivacyEnabled ? user.AccountId : user.Name;
    }
}