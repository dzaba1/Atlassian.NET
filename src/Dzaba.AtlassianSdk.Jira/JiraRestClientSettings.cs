using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Settings to configure the JIRA REST client.
/// </summary>
public class JiraRestClientSettings
{
    /// <summary>
    /// Setup this to trace each request.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; set; }

    internal ILogger<T> GetLogger<T>()
    {
        return LoggerFactory?.CreateLogger<T>();
    }
}
