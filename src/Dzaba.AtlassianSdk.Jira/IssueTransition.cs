using System.Collections.Generic;
using System.Linq;
using Dzaba.AtlassianSdk.Jira.Model.V3;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// An issue transition as defined in JIRA.
/// </summary>
public class IssueTransition : JiraNamedEntity
{
    /// <summary>
    /// Creates an instance of the IssueTransition based on a remote entity.
    /// </summary>
    public IssueTransition(Transition remoteEntity)
        : base(remoteEntity.Id, remoteEntity.Name)
    {
        To = remoteEntity.To == null ? null : new IssueStatus(remoteEntity.To);
        HasScreen = remoteEntity.Screen != null;
        Type = remoteEntity.Type;
        Fields = remoteEntity.fields?.ToDictionary(x => x.Key, x => new IssueFieldEditMetadata(x.Value));
    }

    /// <summary>
    /// Creates an instance of the IssueTransition with the given id and name.
    /// </summary>
    public IssueTransition(string id, string name = null)
        : base(id, name)
    {
    }

    /// <summary>
    /// Gets the status the issue will transition to.
    /// </summary>
    public IssueStatus To { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the transition displays a screen.
    /// </summary>
    public bool HasScreen { get; private set; }

    public TransitionType Type { get; private set; }

    /// <summary>
    /// Gets the field metadata available on the transition screen, keyed by field id.
    /// </summary>
    public Dictionary<string, IssueFieldEditMetadata> Fields { get; private set; }
}
