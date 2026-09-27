using System;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Represents a named entity within JIRA.
/// </summary>
public class JiraNamedEntity : IJiraEntity
{
    /// <summary>
    /// Creates an instance of a JiraNamedEntity.
    /// </summary>
    /// <param name="id">Identifier of the entity.</param>
    /// <param name="name">Name of the entity.</param>
    public JiraNamedEntity(string id, string name = null)
    {
        if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException($"Named entity should have and id or a name. Id: '{id}'. Name: '{name}'.");
        }

        Id = id;
        Name = name;
    }

    /// <summary>
    /// Id of the entity.
    /// </summary>
    public string Id { get; protected set; }

    /// <summary>
    /// Name of the entity.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Returns the name of the entity, or its id if the name is not set.
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }
        else
        {
            return Id;
        }
    }
}
