using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira;

/// <summary>
/// Represents a named entity within JIRA.
/// </summary>
public class JiraNamedEntity : IJiraEntity
{
    /// <summary>
    /// Creates an instance of a JiraNamedEntity base on a remote entity.
    /// </summary>
    public JiraNamedEntity(AbstractNamedRemoteEntity remoteEntity)
        : this(remoteEntity.id, remoteEntity.name)
    {
    }

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
    /// Retrieves the entities of this type from JIRA, used to resolve an id/name pair that is missing one of the two.
    /// </summary>
    /// <param name="jira">The JIRA instance to query.</param>
    /// <param name="token">A token to cancel the operation.</param>
    protected virtual IAsyncEnumerable<JiraNamedEntity> GetEntitiesAsync(IJira jira, CancellationToken token)
    {
        throw new NotImplementedException();
    }

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

    internal async Task<JiraNamedEntity> LoadIdAndNameAsync(IJira jira, CancellationToken token)
    {
        if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Name))
        {
            var entities = await GetEntitiesAsync(jira, token)
                .ToArrayAsync()
                .ConfigureAwait(false);
            var entity = entities.FirstOrDefault(e =>
                (!string.IsNullOrEmpty(Name) && string.Equals(e.Name, Name, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(Id) && string.Equals(e.Id, Id, StringComparison.OrdinalIgnoreCase)));

            if (entity == null)
            {
                throw new InvalidOperationException(string.Format("Entity with id '{0}' and name '{1}' was not found for type '{2}'. Available: [{3}]",
                    Id,
                    Name,
                    GetType(),
                    string.Join(",", entities.Select(s => s.Id + ":" + s.Name).ToArray())));
            }

            Id = entity.Id;
            Name = entity.Name;
        }

        return this;
    }
}
