using System.Linq;
using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using FluentAssertions;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class JqlQueryProviderTest
{
    [Test]
    public void Count()
    {
        var jira = TestableJira.Create();
        var provider = new JiraQueryProvider(jira.Translator.Object, jira.IssueService.Object);
        var queryable = new JiraQueryable<Issue>(provider);

        jira.SetupIssues(new RemoteIssue());

        queryable.Count().Should().Be(1);
    }

    [Test]
    public void First()
    {
        var jira = TestableJira.Create();
        var provider = new JiraQueryProvider(jira.Translator.Object, jira.IssueService.Object);
        var queryable = new JiraQueryable<Issue>(provider);

        jira.SetupIssues(new RemoteIssue() { summary = "foo" }, new RemoteIssue());

        queryable.First().Summary.Should().Be("foo");
    }
}
