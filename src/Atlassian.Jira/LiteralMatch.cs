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

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteralMatch"/> class.
    /// </summary>
    /// <param name="value">The literal value to match exactly.</param>
    public LiteralMatch(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Returns the literal value being matched.
    /// </summary>
    public override string ToString()
    {
        return _value;
    }

    /// <summary>
    /// Determines whether the comparable value equals the literal value; used by the LINQ to JQL provider to force an exact match operator.
    /// </summary>
    /// <param name="comparable">The field value being compared.</param>
    /// <param name="literal">The literal value to match exactly.</param>
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

    /// <summary>
    /// Determines whether the comparable value does not equal the literal value; used by the LINQ to JQL provider to force an exact match operator.
    /// </summary>
    /// <param name="comparable">The field value being compared.</param>
    /// <param name="literal">The literal value to match exactly.</param>
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
