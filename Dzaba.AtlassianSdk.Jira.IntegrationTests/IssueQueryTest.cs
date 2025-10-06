using Atlassian.Jira;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dzaba.AtlassianSdk.Jira.IntegrationTests;

[TestFixture]
public class IssueQueryTest : JiraTestFixture
{
    [Test]
    public async Task GetIssueThatIncludesOnlyOneBasicField()
    {
        var options = new IssueSearchOptions($"key = {TestProject.Key}-1")
        {
            FetchBasicFields = false,
            AdditionalFields = new List<string>() { "summary" }
        };

        var issues = await Jira.Issues.GetIssuesFromJqlAsync(options);

        issues.First().Summary.Should().NotBeNullOrEmpty();
        issues.First().Assignee.Should().BeNull();
    }
}
