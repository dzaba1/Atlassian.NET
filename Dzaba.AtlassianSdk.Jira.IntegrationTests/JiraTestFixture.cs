using Atlassian.Jira;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

public class JiraTestFixture
{
    private static readonly string ClouldUrl = "";
    private static readonly string ClouldUser = "";
    private static readonly string ClouldToken = "";
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

        Jira = Atlassian.Jira.Jira.CreateRestClient(ClouldUrl, ClouldUser, ClouldToken, settings);
        TestProjectKey = "";
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
