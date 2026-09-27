namespace Atlassian.Jira.Linq;

/// <summary>
/// Abstracts the translation of an Expression tree into JQL
/// </summary>
public interface IJqlExpressionVisitor
{
    /// <summary>
    /// Translates the given expression tree into JQL.
    /// </summary>
    /// <param name="expression">The expression to translate.</param>
    /// <returns>The resulting JQL data.</returns>
    JqlData Process(System.Linq.Expressions.Expression expression);
}
