namespace Atlassian.Jira.Linq;

/// <summary>
/// Represents the result of translating a LINQ expression tree into a JQL query.
/// </summary>
public class JqlData
{
    /// <summary>
    /// Gets or sets the JQL expression text.
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to return, if a Take was applied.
    /// </summary>
    public int? NumberOfResults { get; set; }

    /// <summary>
    /// Gets or sets the number of results to skip, if a Skip was applied.
    /// </summary>
    public int? SkipResults { get; set; }
}
