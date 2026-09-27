using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Atlassian.Jira.Linq;

/// <summary>
/// A queryable sequence of JIRA issues that translates LINQ expressions into JQL.
/// </summary>
/// <typeparam name="T">The type of the elements in the query, typically <see cref="Issue"/>.</typeparam>
public class JiraQueryable<T>: IOrderedQueryable<T>, IQueryable<T>
{
    private readonly JiraQueryProvider _provider;
    private readonly Expression _expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="JiraQueryable{T}"/> class representing the root of a query.
    /// </summary>
    /// <param name="provider">The provider used to execute the query.</param>
    public JiraQueryable(JiraQueryProvider provider)
    {
        _provider = provider;
        _expression = Expression.Constant(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JiraQueryable{T}"/> class representing a query built up so far.
    /// </summary>
    /// <param name="provider">The provider used to execute the query.</param>
    /// <param name="expression">The expression tree built up so far.</param>
    public JiraQueryable(JiraQueryProvider provider, Expression expression)
    {
        _provider = provider;
        _expression = expression;
    }

    /// <summary>
    /// Executes the query and returns an enumerator over the results.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_provider.Execute(Expression)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_provider.Execute(Expression)).GetEnumerator();
    }

    /// <summary>
    /// Gets the type of the elements in the query.
    /// </summary>
    public Type ElementType
    {
        get
        {
            return typeof(T);
        }
    }

    /// <summary>
    /// Gets the expression tree built up so far for this query.
    /// </summary>
    public Expression Expression
    {
        get
        {
            return _expression;
        }
    }

    /// <summary>
    /// Gets the provider used to execute the query.
    /// </summary>
    public IQueryProvider Provider
    {
        get
        {
            return _provider;
        }
    }
}
