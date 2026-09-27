using System;
using System.Collections.Generic;
using System.Threading;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// The resolution of the issue as defined in JIRA
/// </summary>
public class IssueResolution : JiraNamedEntity, IEquatable<IssueResolution>
{
    /// <summary>
    /// Creates an instance of the IssueResolution with the given id and name.
    /// </summary>
    public IssueResolution(string id, string name = null)
        : base(id, name)
    {
    }

    protected IAsyncEnumerable<JiraNamedEntity> GetEntitiesAsync(IJira jira, CancellationToken token)
    {
        return jira.Resolutions.GetResolutionsAsync(token);
    }

    /// <summary>
    /// Determines whether the specified <see cref="IssueResolution"/> represents the same JIRA resolution.
    /// </summary>
    public bool Equals(IssueResolution other)
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
        return Equals(obj as IssueResolution);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    /// <summary>
    /// Allows assignation by name
    /// </summary>
    public static implicit operator IssueResolution(string name)
    {
        if (name != null)
        {
            int id;
            if (int.TryParse(name, out id))
            {
                return new IssueResolution(name /*as id*/);
            }
            else
            {
                return new IssueResolution(null, name);
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
    public static bool operator ==(IssueResolution entity, string name)
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
    public static bool operator !=(IssueResolution entity, string name)
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
