using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration;

public class IssueQueryTest
{
    private readonly Random _random = new Random();

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssuesFromFilterWithByNameWithFields(Jira jira)
    {
        var issues = await jira.Filters.GetIssuesFromFavoriteWithFieldsAsync("One Issue Filter", fields: new List<string> { "watches" }).ToArrayAsync();

        Assert.Single(issues);
        var issue = issues.First();
        Assert.Equal("TST-1", issue.Key.Value);
        Assert.Null(issue.Summary);
        Assert.True(issue.AdditionalFields.ContainsKey("watches"), "Watches should be included by query.");
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssuesFromFilterById(Jira jira)
    {
        var issues = await jira.Filters.GetIssuesFromFilterAsync("10000").ToArrayAsync();

        Assert.Single(issues);
        var issue = issues.First();
        Assert.Equal("TST-1", issue.Key.Value);
        Assert.NotNull(issue.Summary);
        Assert.False(issue.AdditionalFields.ContainsKey("watches"), "Watches should be excluded by default.");
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssuesFromFilterByIdWithFields(Jira jira)
    {
        var issues = await jira.Filters.GetIssuesFromFilterWithFieldsAsync("10000", fields: new List<string> { "watches" }).ToArrayAsync();

        Assert.Single(issues);
        var issue = issues.First();
        Assert.Equal("TST-1", issue.Key.Value);
        Assert.Null(issue.Summary);
        Assert.True(issue.AdditionalFields.ContainsKey("watches"), "Watches should be included by query.");
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public void QueryWithZeroResults(Jira jira)
    {
        var issues = from i in jira.Issues.Queryable
                     where i.Created == new DateTime(2010, 1, 1)
                     select i;

        Assert.Equal(0, issues.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task QueryIssueWithLabel(Jira jira)
    {
        var issue = new Issue(jira, "TST")
        {
            Type = "1",
            Summary = "Test issue with labels",
            Assignee = "admin"
        };

        issue.Labels.Add("test-label");
        await issue.SaveChangesAsync();

        var serverIssue = (from i in jira.Issues.Queryable
                           where i.Labels == "test-label"
                           select i).First();

        Assert.Contains("test-label", serverIssue.Labels);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task QueryIssuesWithTakeExpression(Jira jira)
    {
        // create 2 issues with same summary
        var randomNumber = _random.Next(int.MaxValue);
        await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync();
        await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync();

        // query with take method to only return 1
        var issues = (from i in jira.Issues.Queryable
                      where i.Summary == randomNumber.ToString()
                      select i).Take(1);

        Assert.Equal(1, issues.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task MaximumNumberOfIssuesPerRequest(Jira jira)
    {
        // create 2 issues with same summary
        var randomNumber = _random.Next(int.MaxValue);
        await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync();
        await (new Issue(jira, "TST") { Type = "1", Summary = "Test Summary " + randomNumber, Assignee = "admin" }).SaveChangesAsync();

        //set maximum issues and query
        jira.Issues.MaxIssuesPerRequest = 1;
        var issues = from i in jira.Issues.Queryable
                     where i.Summary == randomNumber.ToString()
                     select i;

        Assert.Equal(1, issues.Count());

    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssuesFromJqlAsync(Jira jira)
    {
        var issues = await jira.Issues.GetIssuesFromJqlAsync("key = TST-1").ToArrayAsync();
        Assert.Single(issues);
    }
}
