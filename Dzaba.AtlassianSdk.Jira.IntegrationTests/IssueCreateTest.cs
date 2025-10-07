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

            var query = from i in Jira.Issues.Queryable
                        where i.Key == issue.Key
                        select i;
            var issues = ToArrayWithResultsWithWait(query);

            issues.Should().HaveCount(1);
            issues[0].Summary.Should().Be(summaryValue);
            issues[0].Project.Should().Be(TestProject.Key);
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task CreateAndQueryIssueWithAllFieldsSet()
    {
        var summaryValue = "Test Summary " + Rand.Next(int.MaxValue);
        var expectedDueDate = new DateTime(2011, 12, 12);
        var issue = Jira.CreateIssue(TestProject.Key);

        try
        {
            await issue.AffectsVersions.AddAsync("1.0");
            issue.Assignee = "admin";
            await issue.Components.AddAsync("Server");
            await issue.SetCustomFieldAsync("Custom Text Field", "Test Value"); // custom field
            issue.Description = "Test Description";
            issue.DueDate = expectedDueDate;
            issue.Environment = "Test Environment";
            await issue.FixVersions.AddAsync("2.0");
            issue.Priority = "Major";
            issue.Reporter = "admin";
            issue.Summary = summaryValue;
            issue.Type = "Bug";
            issue.Labels.Add("testLabel");

            await issue.SaveChangesAsync();

            var query = from i in Jira.Issues.Queryable
                        where i.Key == issue.Key
                        select i;
            var queriedIssue = ToArrayWithResultsWithWait(query).First();

            queriedIssue.Summary.Should().Be(summaryValue);
            queriedIssue.JiraIdentifier.Should().NotBeNullOrEmpty();
            queriedIssue.DueDate.Value.Should().Be(expectedDueDate);
            queriedIssue.Type.IconUrl.Should().NotBeNullOrEmpty();
            queriedIssue.Status.IconUrl.Should().NotBeNullOrEmpty();
            queriedIssue.Labels.Should().Contain("testLabel");
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task CreateAndQueryIssueWithSubTask()
    {
        var parentTask = Jira.CreateIssue(TestProject.Key);
        try
        {
            parentTask.Type = "Bug";
            parentTask.Summary = "Test issue with SubTask" + Rand.Next(int.MaxValue);
            await parentTask.SaveChangesAsync();

            var subTaskType = await Jira.IssueTypes.GetIssueTypesForProjectAsync(TestProject.Key)
                .FirstAsync(i => i.IsSubTask);

            var subTask = Jira.CreateIssue(TestProject.Key, parentTask.Key.Value);
            subTask.Type = subTaskType;
            subTask.Summary = "Test SubTask" + Rand.Next(int.MaxValue);
            await subTask.SaveChangesAsync();

            parentTask.Type.IsSubTask.Should().BeFalse();
            subTask.Type.IsSubTask.Should().BeTrue();
            subTask.ParentIssueKey.Should().Be(parentTask.Key.Value);

            // query the subtask again to make sure it loads everything from server.
            subTask = await Jira.Issues.GetIssueAsync(subTask.Key.Value);
            subTask.Type.IsSubTask.Should().BeTrue();
            subTask.ParentIssueKey.Should().Be(parentTask.Key.Value);
        }
        finally
        {
            await DeleteIssueSafeAsync(parentTask.Key?.Value);
        }
    }

    [Test]
    public async Task CreateAndQueryIssueWithVersions()
    {
        var summaryValue = "Test issue with versions (Created)" + Rand.Next(int.MaxValue);

        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = summaryValue,
            Assignee = "admin"
        };

        try
        {
            await issue.AffectsVersions.AddAsync("1.0");
            await issue.AffectsVersions.AddAsync("2.0");

            await issue.FixVersions.AddAsync("3.0");
            await issue.FixVersions.AddAsync("2.0");

            await issue.SaveChangesAsync();

            var query = from i in Jira.Issues.Queryable
                         where i.AffectsVersions == "1.0" && i.AffectsVersions == "2.0"
                                 && i.FixVersions == "2.0" && i.FixVersions == "3.0"
                         select i;
            var newIssue = ToArrayWithResultsWithWait(query).First();

            newIssue.AffectsVersions.Should().HaveCount(2);
            newIssue.AffectsVersions.Should().Contain(v => v.Name == "1.0");
            newIssue.AffectsVersions.Should().Contain(v => v.Name == "2.0");

            newIssue.FixVersions.Should().HaveCount(2);
            newIssue.FixVersions.Should().Contain(v => v.Name == "1.0");
            newIssue.FixVersions.Should().Contain(v => v.Name == "2.0");
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task CreateAndQueryIssueWithComponents()
    {
        var summaryValue = "Test issue with components (Created)" + Rand.Next(int.MaxValue);

        var issue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = summaryValue,
            Assignee = "admin"
        };

        try
        {
            await issue.Components.AddAsync("Server");
            await issue.Components.AddAsync("Client");

            await issue.SaveChangesAsync();

            var query = from i in Jira.Issues.Queryable
                        where i.Summary == summaryValue && i.Components == "Server" && i.Components == "Client"
                        select i;
            var newIssue = ToArrayWithResultsWithWait(query).First();

            newIssue.Components.Should().HaveCount(2);
            newIssue.Components.Should().Contain(c => c.Name == "Server");
            newIssue.Components.Should().Contain(c => c.Name == "Client");
        }
        finally
        {
            await DeleteIssueSafeAsync(issue.Key?.Value);
        }
    }

    [Test]
    public async Task CreateIssueAsSubtask()
    {
        var summaryValue = "Test issue as subtask " + Rand.Next(int.MaxValue);

        var parentIssue = new Issue(Jira, TestProject.Key)
        {
            Type = "Bug",
            Summary = summaryValue,
            Assignee = "admin"
        };

        try
        {
            await parentIssue.SaveChangesAsync();

            var subTaskType = await Jira.IssueTypes.GetIssueTypesForProjectAsync(TestProject.Key)
                .FirstAsync(i => i.IsSubTask);

            var issue = new Issue(Jira, TestProject.Key, parentIssue.Key.Value)
            {
                Type = subTaskType,
                Summary = summaryValue,
                Assignee = "admin"
            };
            await issue.SaveChangesAsync();

            var subtasks = await ToArrayWithResultsWithWait(Jira.Issues.GetIssuesFromJqlAsync($"project = {TestProject.Key} and parent = {parentIssue.Key.Value}"));

            subtasks.Any(s => s.Summary.Equals(summaryValue)).Should().BeTrue("'{0}' was not found as a sub-task of TST-1", summaryValue);
        }
        finally
        {
            await DeleteIssueSafeAsync(parentIssue.Key?.Value);
        }
    }
}
