using Atlassian.Jira;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

[TestFixture]
public class IssueQueryTest : TestFixtureWithIssue
{
    [Test]
    public async Task GetIssueThatIncludesOnlyOneBasicField()
    {
        var options = new IssueSearchOptions($"key = {TestIssue.Key.Value}")
        {
            FetchBasicFields = false,
            AdditionalFields = new List<string>() { "summary" }
        };

        var issues = await ToArrayWithResultsWithWait(Jira.Issues.GetIssuesFromJqlAsync(options));

        issues.First().Summary.Should().NotBeNullOrEmpty();
        issues.First().Assignee.Should().BeNull();
    }

    [Test]
    public async Task GetIssueThatIncludesOnlyAllNonBasicFields()
    {
        // Arrange
        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = "Test issue",
            Assignee = "admin"
        };

        try
        {
            await issue.SaveChangesAsync();

            await issue.AddCommentAsync("My comment");
            await issue.AddWorklogAsync("1d");

            // Act
            var options = new IssueSearchOptions($"key = {issue.Key.Value}")
            {
                FetchBasicFields = false,
                AdditionalFields = new List<string>() { "comment", "watches", "worklog" }
            };

            var issues = await ToArrayWithResultsWithWait(Jira.Issues.GetIssuesFromJqlAsync(options));
            var serverIssue = issues.First();

            // Assert
            serverIssue.Summary.Should().NotBeNullOrEmpty();
            serverIssue.AdditionalFields.Should().ContainKey("watches");

            var worklogs = serverIssue.AdditionalFields.Worklogs;
            worklogs.ItemsPerPage.Should().Be(20);
            worklogs.StartAt.Should().Be(0);
            worklogs.TotalItems.Should().Be(1);
            worklogs.First().TimeSpent.Should().Be("1d");

            var comments = serverIssue.AdditionalFields.Comments;
            comments.ItemsPerPage.Should().Be(1);
            comments.StartAt.Should().Be(0);
            comments.TotalItems.Should().Be(1);
            comments.First().Body.Should().Be("My comment");
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task GetIssuesAsyncWhenIssueDoesNotExist()
    {
        var dict = await Jira.Issues.GetIssuesAsync("NAN-9999");

        dict.Should().NotContainKey("NAN-9999");
    }
}
