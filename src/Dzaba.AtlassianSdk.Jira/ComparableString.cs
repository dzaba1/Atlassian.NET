using System;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Type that wraps a string and exposes operator overloads for
/// easier LINQ queries
/// </summary>
/// <remarks>
/// Allows comparisons in the form of issue.Key > "TST-1"
/// </remarks>
public class ComparableString
{
    /// <summary>
    /// Gets or sets the wrapped string value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComparableString"/> class.
    /// </summary>
    /// <param name="value">The string value to wrap.</param>
    public ComparableString(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Implicitly wraps a string in a <see cref="ComparableString"/>.
    /// </summary>
    /// <param name="value">The string value to wrap.</param>
    public static implicit operator ComparableString(string value)
    {
        if (value != null)
        {
            return new ComparableString(value);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether the wrapped value equals the given string.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The string to compare against.</param>
    public static bool operator ==(ComparableString field, string value)
    {
        if ((object)field == null)
        {
            return value == null;
        }
        else
        {
            return field.Value == value;
        }
    }

    /// <summary>
    /// Determines whether the wrapped value does not equal the given string.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The string to compare against.</param>
    public static bool operator !=(ComparableString field, string value)
    {
        if ((object)field == null)
        {
            return value != null;
        }
        else
        {
            return field.Value != value;
        }
    }

    /// <summary>
    /// Determines whether the wrapped value is greater than the given string.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The string to compare against.</param>
    public static bool operator >(ComparableString field, string value)
    {
        return field.Value.CompareTo(value) > 0;
    }

    /// <summary>
    /// Determines whether the wrapped value is less than the given string.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The string to compare against.</param>
    public static bool operator <(ComparableString field, string value)
    {
        return field.Value.CompareTo(value) < 0;
    }

    /// <summary>
    /// Determines whether the wrapped value is less than or equal to the given string.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The string to compare against.</param>
    public static bool operator <=(ComparableString field, string value)
    {
        return field.Value.CompareTo(value) <= 0;
    }

    /// <summary>
    /// Determines whether the wrapped value is greater than or equal to the given string.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The string to compare against.</param>
    public static bool operator >=(ComparableString field, string value)
    {
        return field.Value.CompareTo(value) >= 0;
    }

    /// <summary>
    /// Determines whether the wrapped value equals the given date, formatted as JIRA expects.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The date to compare against.</param>
    public static bool operator ==(ComparableString field, DateTime value)
    {
        if ((object)field == null)
        {
            return false;
        }
        else
        {
            return field.Value == Jira.FormatDateTimeString(value);
        }
    }

    /// <summary>
    /// Determines whether the wrapped value does not equal the given date, formatted as JIRA expects.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The date to compare against.</param>
    public static bool operator !=(ComparableString field, DateTime value)
    {
        if ((object)field == null)
        {
            return true;
        }
        else
        {
            return field.Value != Jira.FormatDateTimeString(value);
        }
    }

    /// <summary>
    /// Determines whether the wrapped value is greater than the given date, formatted as JIRA expects.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The date to compare against.</param>
    public static bool operator >(ComparableString field, DateTime value)
    {
        return field.Value.CompareTo(Jira.FormatDateTimeString(value)) > 0;
    }

    /// <summary>
    /// Determines whether the wrapped value is less than the given date, formatted as JIRA expects.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The date to compare against.</param>
    public static bool operator <(ComparableString field, DateTime value)
    {
        return field.Value.CompareTo(Jira.FormatDateTimeString(value)) < 0;
    }

    /// <summary>
    /// Determines whether the wrapped value is less than or equal to the given date, formatted as JIRA expects.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The date to compare against.</param>
    public static bool operator <=(ComparableString field, DateTime value)
    {
        return field.Value.CompareTo(Jira.FormatDateTimeString(value)) <= 0;
    }

    /// <summary>
    /// Determines whether the wrapped value is greater than or equal to the given date, formatted as JIRA expects.
    /// </summary>
    /// <param name="field">The value to compare.</param>
    /// <param name="value">The date to compare against.</param>
    public static bool operator >=(ComparableString field, DateTime value)
    {
        return field.Value.CompareTo(Jira.FormatDateTimeString(value)) >= 0;
    }

    /// <summary>
    /// Returns the wrapped string value.
    /// </summary>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the wrapped value.
    /// </summary>
    /// <param name="obj">A <see cref="ComparableString"/> or <see cref="string"/> to compare against.</param>
    public override bool Equals(object obj)
    {
        if (obj is ComparableString)
        {
            return Value.Equals(((ComparableString)obj).Value);
        }
        else if (obj is string)
        {
            return Value.Equals((string)obj);
        }

        return base.Equals(obj);
    }

    /// <summary>
    /// Returns a hash code based on the wrapped string value.
    /// </summary>
    public override int GetHashCode()
    {
        if (Value == null)
        {
            return 0;
        }
        return Value.GetHashCode();
    }
}
