using System.Diagnostics.CodeAnalysis;

namespace Atlassian.Jira;

/// <summary>
/// Force a CustomField comparison to use the exact match JQL operator.
/// </summary>
[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class LiteralMatch
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    private readonly string _value;

    public LiteralMatch(string value)
    {
        this._value = value;
    }

    public override string ToString()
    {
        return _value;
    }

    public static bool operator ==(ComparableString comparable, LiteralMatch literal)
    {
        if ((object)comparable == null)
        {
            return literal == null;
        }
        else
        {
            return comparable.Value == literal._value;
        }
    }

    public static bool operator !=(ComparableString comparable, LiteralMatch literal)
    {
        if ((object)comparable == null)
        {
            return literal != null;
        }
        else
        {
            return comparable.Value != literal._value;
        }
    }
}
