using System;
using System.Collections.Generic;

namespace Dzaba.AtlassianSdk.Jira;

internal class JiraEntityNameEqualityComparer : IEqualityComparer<JiraNamedEntity>
{
    public static JiraEntityNameEqualityComparer Instance { get; } = new JiraEntityNameEqualityComparer();

    public bool Equals(JiraNamedEntity x, JiraNamedEntity y)
    {
        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(JiraNamedEntity obj)
    {
        if (obj == null || obj.Name == null)
        {
            return 0;
        }

        return obj.Name.GetHashCode();
    }
}
