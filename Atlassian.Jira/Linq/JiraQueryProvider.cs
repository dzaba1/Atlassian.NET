using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Atlassian.Jira.Linq;

/// <summary>
/// Translates LINQ expression trees into JQL and executes them against JIRA.
/// </summary>
public class JiraQueryProvider : IQueryProvider
{
    private readonly IJqlExpressionVisitor _translator;
    private readonly IIssueService _issues;

    /// <summary>
    /// Initializes a new instance of the <see cref="JiraQueryProvider"/> class.
    /// </summary>
    /// <param name="translator">Translates expression trees into JQL.</param>
    /// <param name="issues">The service used to run JQL queries against JIRA.</param>
    public JiraQueryProvider(IJqlExpressionVisitor translator, IIssueService issues)
    {
        _translator = translator;
        _issues = issues;
    }

    /// <inheritdoc/>
    public IQueryable<T> CreateQuery<T>(Expression expression)
    {
        return new JiraQueryable<T>(this, expression);
    }

    /// <inheritdoc/>
    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public T Execute<T>(Expression expression)
    {
        bool isEnumerable = (typeof(T).Name == "IEnumerable`1");

        return (T)ExecuteAsync(expression, isEnumerable).Result;
    }

    /// <inheritdoc/>
    public object Execute(Expression expression)
    {
        return ExecuteAsync(expression, true).Result;
    }

    private async Task<object> ExecuteAsync(Expression expression, bool isEnumerable)
    {
        var jql = _translator.Process(expression);

        var temp = _issues.GetIssuesFromJqlAsync(jql.Expression);

        if (jql.SkipResults != null)
        {
            temp = temp.Skip(jql.SkipResults.Value);
        }

        if (jql.NumberOfResults != null)
        {
            temp = temp.Take(jql.NumberOfResults.Value);
        }

        var array = await temp.ToArrayAsync();
        IQueryable<Issue> issues = array.AsQueryable();

        if (isEnumerable)
        {
            return issues;
        }
        else
        {
            var treeCopier = new ExpressionTreeModifier(issues);
            Expression newExpressionTree = treeCopier.Visit(expression);

            return issues.Provider.Execute(newExpressionTree);
        }
    }
}
