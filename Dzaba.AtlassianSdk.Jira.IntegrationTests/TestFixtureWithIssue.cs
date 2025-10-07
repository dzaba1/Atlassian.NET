using Atlassian.Jira;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

public abstract class TestFixtureWithIssue : JiraTestFixture
{
    protected Issue TestIssue { get; private set; }

    [OneTimeSetUp]
    public async Task OneTimeIssueSetup()
    {
        TestIssue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = "Test summary",
            Assignee = "admin"
        };

        await TestIssue.SaveChangesAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeIssueCleanUp()
    {
        await DeleteIssueSafeAsync(TestIssue.Key?.Value);
    }
}
