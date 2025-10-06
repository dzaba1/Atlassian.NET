using Atlassian.Jira;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

[TestFixture]
public class IssueCreateTest : JiraTestFixture
{
    [Test]
    public async Task CreateIssueWithIssueTypesPerProject()
    {
        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = "Test Summary " + Rand.Next(int.MaxValue),
            Assignee = "admin"
        };

        try
        {
            issue.Type.SearchByProjectOnly = true;
            var newIssue = await issue.SaveChangesAsync();

            newIssue.Project.Should().Be(TestProject.Key);
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }
}
