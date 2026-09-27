using System;
using System.Collections.Generic;
using System.Threading;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// The priority of the issue as defined in JIRA
/// </summary>
public class IssuePriority : JiraNamedConstant, IEquatable<IssuePriority>
{
    /// <summary>
    /// Creates an instance of the IssuePriority based on a remote entity.
    /// </summary>
    public IssuePriority(Priority remoteEntity)
        : base(remoteEntity.Id, remoteEntity.Description, remoteEntity.IconUrl, remoteEntity.Name)
    {
    }

    /// <summary>
    /// Creates an instance of the IssuePriority with the given id and name.
    /// </summary>
    public IssuePriority(string id, string name = null)
        : base(id, null, null, name)
    {
    }

    protected IAsyncEnumerable<JiraNamedEntity> GetEntitiesAsync(IJira jira, CancellationToken token)
    {
        return jira.Priorities.GetPrioritiesAsync(token);
    }

    /// <summary>
    /// Determines whether the specified <see cref="IssuePriority"/> represents the same JIRA priority.
    /// </summary>
    public bool Equals(IssuePriority other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Id, other.Id, StringComparison.Ordinal)
            && string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return Equals(obj as IssuePriority);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    /// <summary>
    /// Allows assignation by name
    /// </summary>
    public static implicit operator IssuePriority(string name)
    {
        if (name != null)
        {
            int id;
            if (int.TryParse(name, out id))
            {
                return new IssuePriority(name /*as id*/);
            }
            else
            {
                return new IssuePriority(null, name);
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Operator overload to simplify LINQ queries
    /// </summary>
    /// <remarks>
    /// Allows calls in the form of issue.Priority == "High"
    /// </remarks>
    public static bool operator ==(IssuePriority entity, string name)
    {
        if ((object)entity == null)
        {
            return name == null;
        }
        else if (name == null)
        {
            return false;
        }
        else
        {
            return entity.Name == name;
        }
    }

    /// <summary>
    /// Operator overload to simplify LINQ queries
    /// </summary>
    /// <remarks>
    /// Allows calls in the form of issue.Priority != "High"
    /// </remarks>
    public static bool operator !=(IssuePriority entity, string name)
    {
        if ((object)entity == null)
        {
            return name != null;
        }
        else if (name == null)
        {
            return true;
        }
        else
        {
            return entity.Name != name;
        }
    }
}
