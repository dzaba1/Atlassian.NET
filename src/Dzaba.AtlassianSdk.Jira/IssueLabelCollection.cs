using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Collection of labels for an issue.
/// </summary>
public class IssueLabelCollection : List<string>, IEquatable<IssueLabelCollection>, IRemoteIssueFieldProvider
{
    private readonly List<string> _originalLabels;

    /// <summary>
    /// Creates a new instance of IssueLabelCollection.
    /// </summary>
    /// <param name="labels">Labels to seed into this collection</param>
    public IssueLabelCollection(IList<string> labels)
        : base(labels)
    {
        _originalLabels = new List<string>(labels);
    }

    /// <summary>
    /// Adds labels to this collection.
    /// </summary>
    /// <param name="labels">The list of labels to add.</param>
    public void Add(params string[] labels)
    {
        AddRange(labels);
    }

    /// <summary>
    /// Determines whether the collection contains the given label.
    /// </summary>
    /// <param name="list">The collection to search.</param>
    /// <param name="value">The label to look for.</param>
    /// <returns><c>true</c> if the collection contains <paramref name="value"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(IssueLabelCollection list, string value)
    {
        return (object)list == null ? value == null : list.Any(v => v == value);
    }

    /// <summary>
    /// Determines whether the collection does not contain the given label.
    /// </summary>
    /// <param name="list">The collection to search.</param>
    /// <param name="value">The label to look for.</param>
    /// <returns><c>true</c> if the collection does not contain <paramref name="value"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(IssueLabelCollection list, string value)
    {
        return (object)list == null ? value == null : !list.Any(v => v == value);
    }

    /// <summary>
    /// Determines whether the specified collection contains the same labels, in the same order.
    /// </summary>
    public bool Equals(IssueLabelCollection other)
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
        return Equals(obj as IssueLabelCollection);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var label in this)
        {
            hash.Add(label);
        }
        return hash.ToHashCode();
    }

    public IAsyncEnumerable<RemoteFieldValue> GetRemoteFieldValuesAsync(CancellationToken token)
    {
        var fieldValues = new List<RemoteFieldValue>();

        if (_originalLabels.Count() != this.Count() || this.Except(_originalLabels).Any())
        {
            fieldValues.Add(new RemoteFieldValue()
            {
                id = "labels",
                values = ToArray()
            });
        }

        return Task.FromResult(fieldValues.ToArray());
    }
}
