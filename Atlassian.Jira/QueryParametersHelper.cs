using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace Atlassian.Jira;

/// <summary>
/// Helper methods for working with URL query parameters.
/// </summary>
public class QueryParametersHelper
{
    /// <summary>
    /// Gets the parameters from a full query string.
    /// </summary>
    /// <param name="query">The url query.</param>
    /// <returns>List of all parameters within the query.</returns>
    public static IEnumerable<Parameter> GetParametersFromPath(string query)
    {
        var parameters = query.TrimStart('?')
            .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
            {
                var p = s.Split(new[] { '=' }, 2);
                return new Parameter(name: p[0], value: p.Length > 1 ? p[1] : "", type: ParameterType.QueryString);
            });

        return parameters;
    }

    /// <summary>
    /// Represents a single URL query parameter.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">The kind of RestSharp parameter this represents.</param>
        /// <param name="encode">Whether the value should be URL-encoded.</param>
        public Parameter(string name, object value, ParameterType type, bool encode = true)
        {
            Name = name;
            Value = value;
            Type = type;
            Encode = encode;
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Gets the kind of RestSharp parameter this represents.
        /// </summary>
        public ParameterType Type { get; }

        /// <summary>
        /// Gets a value indicating whether the value should be URL-encoded.
        /// </summary>
        public bool Encode { get; }
    }
}
