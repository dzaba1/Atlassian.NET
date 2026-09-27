using System;
using System.Diagnostics.CodeAnalysis;

namespace Atlassian.Jira;

/// <summary>
/// Force a DateTime field to use a string provided as the JQL query value.
/// </summary>
[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class LiteralDateTime
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    private readonly string _dateTimeString;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteralDateTime"/> class.
    /// </summary>
    /// <param name="dateTimeString">The literal string to use as the JQL query value.</param>
    public LiteralDateTime(string dateTimeString)
    {
        _dateTimeString = dateTimeString;
    }

    /// <summary>
    /// Returns the literal string that will be used as the JQL query value.
    /// </summary>
    public override string ToString()
    {
        return _dateTimeString;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator ==(DateTime dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator !=(DateTime dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator >(DateTime dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator <(DateTime dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator >=(DateTime dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator <=(DateTime dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator ==(DateTime? dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator !=(DateTime? dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator >(DateTime? dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator <(DateTime? dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator >=(DateTime? dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }

    /// <summary>
    /// Present only so the LINQ to JQL provider can translate the comparison; not intended to be evaluated at runtime and always returns <c>false</c>.
    /// </summary>
    public static bool operator <=(DateTime? dateTime, LiteralDateTime literalDateTime)
    {
        return false;
    }
}
