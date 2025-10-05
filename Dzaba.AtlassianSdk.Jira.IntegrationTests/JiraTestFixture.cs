using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

public class JiraTestFixture
{
    private static readonly string ClouldUrl = "";
    private static readonly string ClouldUser = "";
    private static readonly string ClouldToken = "";
    protected static readonly Random Rand = new Random();

    protected Atlassian.Jira.Jira Jira { get; private set; }
    protected string TestProjectKey { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetupBase()
    {
        Jira = Atlassian.Jira.Jira.CreateRestClient(ClouldUrl, ClouldUser, ClouldToken);
        TestProjectKey = "";
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
