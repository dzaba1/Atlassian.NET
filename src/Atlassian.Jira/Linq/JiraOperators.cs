namespace Atlassian.Jira.Linq;

/// <summary>
/// Container for the supported JIRA operator strings.
/// </summary>
public class JiraOperators
{
    /// <summary>
    /// The JQL equality operator ("=").
    /// </summary>
    public const string EQUALS = "=";

    /// <summary>
    /// The JQL inequality operator ("!=").
    /// </summary>
    public const string NOTEQUALS = "!=";

    /// <summary>
    /// The JQL "contains" (text search) operator ("~").
    /// </summary>
    public const string CONTAINS = "~";

    /// <summary>
    /// The JQL "does not contain" (text search) operator ("!~").
    /// </summary>
    public const string NOTCONTAINS = "!~";

    /// <summary>
    /// The JQL "is" operator, used for comparing against EMPTY or NULL ("is").
    /// </summary>
    public const string IS = "is";

    /// <summary>
    /// The JQL "is not" operator, used for comparing against EMPTY or NULL ("is not").
    /// </summary>
    public const string ISNOT = "is not";

    /// <summary>
    /// The JQL "greater than" operator (">").
    /// </summary>
    public const string GREATERTHAN = ">";

    /// <summary>
    /// The JQL "less than" operator ("&lt;").
    /// </summary>
    public const string LESSTHAN = "<";

    /// <summary>
    /// The JQL "greater than or equal to" operator (">=").
    /// </summary>
    public const string GREATERTHANOREQUALS = ">=";

    /// <summary>
    /// The JQL "less than or equal to" operator ("&lt;=").
    /// </summary>
    public const string LESSTHANOREQUALS = "<=";

    /// <summary>
    /// The JQL logical "or" operator ("or").
    /// </summary>
    public const string OR = "or";

    /// <summary>
    /// The JQL logical "and" operator ("and").
    /// </summary>
    public const string AND = "and";
}
