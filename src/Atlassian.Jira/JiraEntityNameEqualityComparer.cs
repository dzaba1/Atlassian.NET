using System;
using System.Collections.Generic;

namespace Atlassian.Jira;

internal class JiraEntityNameEqualityComparer : IEqualityComparer<JiraNamedEntity>
{
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
