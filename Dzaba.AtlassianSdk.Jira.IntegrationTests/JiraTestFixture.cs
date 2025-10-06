using Atlassian.Jira;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

public abstract class JiraTestFixture
{
    private static readonly string CloudUrlEnvKey = "TEST_JIRA_URL";
    private static readonly string CloudUserEnvKey = "TEST_JIRA_USER";
    private static readonly string CloudTokenEnvKey = "TEST_JIRA_TOKEN";
    private static readonly string CloudTestProjectEnvKey = "TEST_JIRA_PROJECT_KEY";

    protected static readonly Random Rand = new Random();

    private ILoggerFactory loggerFactory;

    protected Atlassian.Jira.Jira Jira { get; private set; }
    protected string TestProjectKey { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetupBase()
    {
        loggerFactory = GetLoggerFactory();

        var settings = new JiraRestClientSettings
        {
            LoggerFactory = loggerFactory
        };

        Jira = Atlassian.Jira.Jira.CreateRestClient(GetValueFromEnv(CloudUrlEnvKey),
            GetValueFromEnv(CloudUserEnvKey),
            GetValueFromEnv(CloudTokenEnvKey),
            settings);

        TestProjectKey = GetValueFromEnv(CloudTestProjectEnvKey);
    }

    private string GetValueFromEnv(string envKey)
    {
        var value = Environment.GetEnvironmentVariable(envKey);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(envKey, $"The environmental variable {envKey} is not set.");
        }
        return value;
    }

    [OneTimeTearDown]
    public void OneTimeClenupBase()
    {
        loggerFactory?.Dispose();
    }

    private ILoggerFactory GetLoggerFactory()
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        return LoggerFactory.Create(l => l.AddSerilog(logger, true));
    }

    protected async Task DeleteIssueSafeAsync(string issueKey)
    {
        if (string.IsNullOrWhiteSpace(issueKey))
        {
            return;
        }

        try
        {
            await Jira.Issues.DeleteIssueAsync(issueKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
