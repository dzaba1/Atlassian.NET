using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira;

/// <summary>
/// A collection of named entities (e.g. components, versions) associated with an issue field.
/// </summary>
/// <typeparam name="T">The type of named entity contained in the collection.</typeparam>
[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class JiraNamedEntityCollection<T> : Collection<T>, IRemoteIssueFieldProvider where T : JiraNamedEntity
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
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

    Task<RemoteFieldValue[]> IRemoteIssueFieldProvider.GetRemoteFieldValuesAsync(CancellationToken token)
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
