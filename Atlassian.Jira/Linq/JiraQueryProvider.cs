using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Atlassian.Jira.Linq;

public class JiraQueryProvider : IQueryProvider
{
    private readonly IJqlExpressionVisitor _translator;
    private readonly IIssueService _issues;

    public JiraQueryProvider(IJqlExpressionVisitor translator, IIssueService issues)
    {
        _translator = translator;
        _issues = issues;
    }

    public IQueryable<T> CreateQuery<T>(Expression expression)
    {
        return new JiraQueryable<T>(this, expression);
    }

    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    public T Execute<T>(Expression expression)
    {
        bool isEnumerable = (typeof(T).Name == "IEnumerable`1");

        return (T)ExecuteAsync(expression, isEnumerable).Result;
    }

    public object Execute(Expression expression)
    {
        return ExecuteAsync(expression, true).Result;
    }

    private async Task<object> ExecuteAsync(Expression expression, bool isEnumerable)
    {
        var jql = _translator.Process(expression);

        var temp = await _issues.GetIssuesFromJqlAsync(jql.Expression, jql.NumberOfResults, jql.SkipResults ?? 0);
        IQueryable<Issue> issues = temp.AsQueryable();

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
