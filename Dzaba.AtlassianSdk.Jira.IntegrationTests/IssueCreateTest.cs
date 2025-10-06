using Atlassian.Jira;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

[TestFixture]
public class IssueCreateTest : JiraTestFixture
{
    [Test]
    public async Task CreateIssueWithIssueTypesPerProject()
    {
        var projIssueType = await Jira.IssueTypes.GetIssueTypesForProjectAsync(TestProject.Key)
            .FirstAsync(i => i.Name == "Bug");
        var issueType = await Jira.IssueTypes.GetIssueTypesAsync()
            .FirstAsync(i => i.Id == projIssueType.Id);

        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = projIssueType,
            Summary = "Test Summary " + Rand.Next(int.MaxValue),
            Assignee = "admin"
        };

        try
        {
            issue.Type.SearchByProjectOnly = true;
            var newIssue = await issue.SaveChangesAsync();

            newIssue.Type.Name.Should().Be(issueType.Name);
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }
}
