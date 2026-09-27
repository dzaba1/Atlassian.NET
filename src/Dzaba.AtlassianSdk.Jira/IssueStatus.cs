using System;
using System.Collections.Generic;
using System.Threading;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// The status of the issue as defined in JIRA
/// </summary>
public class IssueStatus : JiraNamedConstant, IEquatable<IssueStatus>
{
    /// <summary>
    /// Creates an instance of the IssueStatus based on a remote entity.
    /// </summary>
    public IssueStatus(StatusDetails remoteStatus)
        : base(remoteStatus.Id, remoteStatus.Description, remoteStatus.IconUrl, remoteStatus.Name)
    {
        StatusCategory = remoteStatus.StatusCategory != null ?
            new IssueStatusCategory(remoteStatus.StatusCategory) :
            null;
    }

/// <summary>
    /// Creates an instance of the IssueStatus based on a remote entity.
    /// </summary>
    public IssueStatus(IssueStatus remoteStatus)
        : base(remoteStatus.Id, remoteStatus.Description, remoteStatus.IconUrl, remoteStatus.Name)
    {
        StatusCategory = remoteStatus.StatusCategory != null ?
            new IssueStatusCategory(remoteStatus.StatusCategory) :
            null;
    }

    internal IssueStatus(string id, string name = null)
        : base(id, null, null, name)
    {
    }

    /// <inheritdoc/>
    protected override IAsyncEnumerable<JiraNamedEntity> GetEntitiesAsync(Jira jira, CancellationToken token)
    {
        return jira.Statuses.GetStatusesAsync(token);
    }

    /// <summary>
    /// The category assigned to this issue status.
    /// </summary>
    public IssueStatusCategory StatusCategory { get; }

    /// <summary>
    /// Allows assignation by name
    /// </summary>
    public static implicit operator IssueStatus(string name)
    {
        if (name != null)
        {
            int id;
            if (int.TryParse(name, out id))
            {
                return new IssueStatus(name /*as id*/);
            }
            else
            {
                return new IssueStatus(null, name);
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
    public static bool operator ==(IssueStatus entity, string name)
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
    public static bool operator !=(IssueStatus entity, string name)
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
