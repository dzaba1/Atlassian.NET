using Atlassian.Jira;
using FluentAssertions;
using NUnit.Framework;
using System;
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

    [Test]
    public async Task CreateIssueWithOriginalEstimate()
    {
        var fields = new CreateIssueFields(TestProject.Key)
        {
            TimeTrackingData = new IssueTimeTrackingData("1d")
        };

        var issue = new Issue(Jira, fields)
        {
            Type = "Bug",
            Summary = "Test Summary " + Rand.Next(int.MaxValue),
            Assignee = "admin"
        };

        try
        {
            var newIssue = await issue.SaveChangesAsync();

            newIssue.TimeTrackingData.OriginalEstimate.Should().Be("1d");
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task CreateIssueAsync()
    {
        var summaryValue = "Test Summary " + Rand.Next(int.MaxValue);
        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = summaryValue,
            Assignee = "admin"
        };

        var subTaskType = await Jira.IssueTypes.GetIssueTypesForProjectAsync(TestProject.Key)
            .FirstAsync(i => i.IsSubTask);

        try
        {
            var newIssue = await issue.SaveChangesAsync();
            newIssue.Summary.Should().Be(summaryValue);
            newIssue.Project.Should().Be(TestProject.Key);

            // Create a subtask async.
            var subTask = new Issue(Jira, TestProject.Key, newIssue.Key.Value)
            {
                Type = subTaskType,
                Summary = "My Subtask",
                Assignee = "admin"
            };

            var newSubTask = await subTask.SaveChangesAsync();

            newSubTask.ParentIssueKey.Should().Be(newIssue.Key.Value);
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task CreateAndQueryIssueWithMinimumFieldsSet()
    {
        var summaryValue = "Test Summary " + Rand.Next(int.MaxValue);

        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = summaryValue,
            Assignee = "admin"
        };

        try
        {
            await issue.SaveChangesAsync();

            // We must wait few seconds otherwise the issue won't be find by JQL
            await Task.Delay(TimeSpan.FromSeconds(2));

            var issues = (from i in Jira.Issues.Queryable
                          where i.Key == issue.Key
                          select i).ToArray();

            issues.Should().HaveCount(1);
            issues[0].Summary.Should().Be(summaryValue);
            issues[0].Project.Should().Be(TestProject.Key);
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }
}
