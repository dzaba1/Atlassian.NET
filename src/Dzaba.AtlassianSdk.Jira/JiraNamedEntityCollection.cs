using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// A collection of named entities (e.g. components, versions) associated with an issue field.
/// </summary>
/// <typeparam name="T">The type of named entity contained in the collection.</typeparam>
public class JiraNamedEntityCollection<T> : Collection<T>, IEquatable<JiraNamedEntityCollection<T>>, IRemoteIssueFieldProvider where T : JiraNamedEntity
{
    /// <summary>
    /// The JIRA instance that owns the collection.
    /// </summary>
    protected readonly IJira _jira;

    /// <summary>
    /// The key of the project the collection belongs to.
    /// </summary>
    protected readonly string _projectKey;

    /// <summary>
    /// The name of the remote field represented by this collection.
    /// </summary>
    protected readonly string _fieldName;

    private readonly List<T> _originalList;

    internal JiraNamedEntityCollection(string fieldName, IJira jira, string projectKey, IList<T> list)
        : base(list)
    {
        _fieldName = fieldName;
        _jira = jira;
        _projectKey = projectKey;
        _originalList = new List<T>(list);
    }

    /// <summary>
    /// Determines whether the collection contains an entity with the given name.
    /// </summary>
    /// <param name="list">The collection to search.</param>
    /// <param name="value">The entity name to look for.</param>
    public static bool operator ==(JiraNamedEntityCollection<T> list, string value)
    {
        return (object)list == null ? value == null : list.Any(v => v.Name == value);
    }

    /// <summary>
    /// Determines whether the collection does not contain an entity with the given name.
    /// </summary>
    /// <param name="list">The collection to search.</param>
    /// <param name="value">The entity name to look for.</param>
    public static bool operator !=(JiraNamedEntityCollection<T> list, string value)
    {
        return (object)list == null ? value == null : !list.Any(v => v.Name == value);
    }

    /// <summary>
    /// Removes an entity by name.
    /// </summary>
    /// <param name="name">Entity name.</param>
    public void Remove(string name)
    {
        Remove(Items.First(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Determines whether the specified collection contains the same entities, in the same order.
    /// </summary>
    public bool Equals(JiraNamedEntityCollection<T> other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.SequenceEqual(other);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return Equals(obj as JiraNamedEntityCollection<T>);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in Items)
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }

    public IAsyncEnumerable<RemoteFieldValue> GetRemoteFieldValuesAsync(CancellationToken token)
    {
        var fields = new List<RemoteFieldValue>();

        if (_originalList.Count() != Items.Count() || _originalList.Except(Items).Any())
        {
            var field = new RemoteFieldValue()
            {
                id = _fieldName,
                values = Items.Select(e => e.Id).ToArray()
            };
            fields.Add(field);
        }

        return Task.FromResult(fields.ToArray());
    }
}
