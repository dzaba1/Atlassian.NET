using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using Atlassian.Jira.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class JqlExpressionTranslatorTest
{
    private JqlExpressionVisitor _translator;

    private JiraQueryable<Issue> CreateQueryable()
    {

        _translator = new JqlExpressionVisitor();

        var jira = Jira.CreateRestClient("http://foo");
        var issues = new Mock<IIssueService>();
        var provider = new JiraQueryProvider(_translator, issues.Object);

        issues.SetupIssues(jira, new RemoteIssue());

        return new JiraQueryable<Issue>(provider);
    }

    [Test]
    public void EqualsOperatorForNonString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes == 5
                      select i).ToArray();

        _translator.Jql.Should().Be("Votes = 5");
    }

    [Test]
    public void EqualsOperatorForStringWithFuzzyEquality()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Summary == "Foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("Summary ~ \"Foo\"");
    }

    [Test]
    public void EqualsOperatorForString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Assignee == "Foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("Assignee = \"Foo\"");
    }

    [Test]
    public void NotEqualsOperatorForNonString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes != 5
                      select i).ToArray();

        _translator.Jql.Should().Be("Votes != 5");
    }

    [Test]
    public void NotEqualsOperatorForStringWithFuzzyEquality()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Summary != "Foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("Summary !~ \"Foo\"");
    }

    [Test]
    public void NotEqualsOperatorForString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Assignee != "Foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("Assignee != \"Foo\"");
    }

    [Test]
    public void GreaterThanOperator()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes > 5
                      select i).ToArray();

        _translator.Jql.Should().Be("Votes > 5");
    }

    [Test]
    public void GreaterThanEqualsOperator()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes >= 5
                      select i).ToArray();

        _translator.Jql.Should().Be("Votes >= 5");
    }

    [Test]
    public void LessThanOperator()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes < 5
                      select i).ToArray();

        _translator.Jql.Should().Be("Votes < 5");
    }

    [Test]
    public void LessThanOrEqualsOperator()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes <= 5
                      select i).ToArray();

        _translator.Jql.Should().Be("Votes <= 5");
    }

    [Test]
    public void AndKeyWord()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes > 5 && i.Votes < 10
                      select i).ToArray();

        _translator.Jql.Should().Be("(Votes > 5 and Votes < 10)");
    }

    [Test]
    public void OrKeyWord()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes > 5 || i.Votes < 10
                      select i).ToArray();

        _translator.Jql.Should().Be("(Votes > 5 or Votes < 10)");
    }

    [Test]
    public void AssociativeGrouping()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Votes > 5 && (i.Votes < 10 || i.Votes == 20)
                      select i).ToArray();

        _translator.Jql.Should().Be("(Votes > 5 and (Votes < 10 or Votes = 20))");
    }

    [Test]
    public void IsOperatorForEmptyString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Summary == ""
                      select i).ToArray();

        _translator.Jql.Should().Be("Summary is empty");
    }

    [Test]
    public void IsNotOperatorForEmptyString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Summary != ""
                      select i).ToArray();

        _translator.Jql.Should().Be("Summary is not empty");
    }

    [Test]
    public void IsOperatorForNull()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Summary == null
                      select i).ToArray();

        _translator.Jql.Should().Be("Summary is null");
    }

    [Test]
    public void GreaterThanOperatorWhenUsingComparableFieldWithString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Priority > "foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("Priority > \"foo\"");
    }

    [Test]
    public void EqualsOperatorWhenUsingComparableFieldWithString()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Priority == "foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("Priority = \"foo\"");
    }

    [Test]
    public void OrderBy()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Priority == "1"
                      orderby i.Created
                      select i).ToArray();

        _translator.Jql.Should().Be("Priority = \"1\" order by Created asc");
    }

    [Test]
    public void OrderByDescending()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Priority == "1"
                      orderby i.Created descending
                      select i).ToArray();

        _translator.Jql.Should().Be("Priority = \"1\" order by Created desc");
    }

    [Test]
    public void MultipleOrderBys()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Priority == "1"
                      orderby i.Created, i.DueDate
                      select i).ToArray();

        _translator.Jql.Should().Be("Priority = \"1\" order by Created asc, DueDate asc");
    }

    [Test]
    public void MultipleOrderByDescending()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Priority == "1"
                      orderby i.Created, i.DueDate descending
                      select i).ToArray();

        _translator.Jql.Should().Be("Priority = \"1\" order by Created asc, DueDate desc");
    }

    [Test]
    public void NewDateTime()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Created > new DateTime(2011, 1, 1)
                      select i).ToArray();

        _translator.Jql.Should().Be("Created > \"2011/01/01\"");
    }

    [Test]
    public void MultipleDateTimes()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Created > new DateTime(2011, 1, 1) && i.Created < new DateTime(2012, 1, 1)
                      select i).ToArray();

        _translator.Jql.Should().Be("(Created > \"2011/01/01\" and Created < \"2012/01/01\")");
    }

    [Test]
    public void LocalStringVariables()
    {
        var queryable = CreateQueryable();
        var user = "farmas";

        var issues = (from i in queryable
                      where i.Assignee == user
                      select i).ToArray();

        _translator.Jql.Should().Be("Assignee = \"farmas\"");
    }

    [Test]
    public void LocalDateVariables()
    {
        var queryable = CreateQueryable();
        var date = new DateTime(2011, 1, 1);

        var issues = (from i in queryable
                      where i.Created > date
                      select i).ToArray();

        _translator.Jql.Should().Be("Created > \"2011/01/01\"");
    }

    [Test]
    public void DateTimeWithLiteralString()
    {
        var queryable = CreateQueryable();
        var date = new DateTime(2011, 1, 1);

        var issues = (from i in queryable
                      where i.Created > new LiteralDateTime(date.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture))
                      select i).ToArray();

        _translator.Jql.Should().Be("Created > \"2011/01/01 00:00\"");
    }

    [Test]
    // https://bitbucket.org/farmas/atlassian.net-sdk/issue/31
    public void DateTimeFormattedAsEnUs()
    {
        var currentCulture = Thread.CurrentThread.CurrentCulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var queryable = CreateQueryable();
            var date = new DateTime(2011, 1, 1);

            var issues = (from i in queryable
                          where i.Created > date
                          select i).ToArray();

            _translator.Jql.Should().Be("Created > \"2011/01/01\"");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }
    }

    [Test]
    public void DateNow()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Created > DateTime.Now.Date
                      select i).ToArray();

        _translator.Jql.Should().Be("Created > \"" + DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) + "\"");
    }

    [Test]
    public void DateTimeNow()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Created > DateTime.Now
                      select i).ToArray();

        _translator.Jql.Should().Be("Created > \"" + DateTime.Now.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) + "\"");
    }

    [Test]
    public void TakeWithConstant()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Assignee == "foo"
                      select i).Take(50).ToArray();

        _translator.NumberOfResults.Should().Be(50);
    }

    [Test]
    public void SkipWithConstant()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Assignee == "foo"
                      select i).Skip(25).Take(50).ToArray();

        _translator.SkipResults.Should().Be(25);
    }

    [Test]
    public void SkipAndTakeShouldResetOnEveryProcessOperation()
    {
        var queryable = CreateQueryable();

        var issues = (from i in queryable
                      where i.Assignee == "foo"
                      select i).Skip(25).Take(50).ToArray();

        var issues2 = (from i in queryable
                       where i.Assignee == "foo"
                       select i).ToArray();

        _translator.SkipResults.Should().BeNull();
        _translator.NumberOfResults.Should().BeNull();
    }

    [Test]
    public void TakeWithLocalVariable()
    {
        var queryable = CreateQueryable();
        var take = 100;

        var issues = (from i in queryable
                      where i.Assignee == "foo"
                      select i).Take(take).ToArray();

        _translator.NumberOfResults.Should().Be(100);
    }

    [Test]
    public void VersionsEqual()
    {
        var queryable = CreateQueryable();
        var issues = (from i in queryable
                      where i.FixVersions == "1.0" && i.AffectsVersions == "2.0"
                      select i).ToArray();

        _translator.Jql.Should().Be("(FixVersion = \"1.0\" and AffectedVersion = \"2.0\")");
    }

    [Test]
    public void ComponentEqual()
    {
        var queryable = CreateQueryable();
        var issues = (from i in queryable
                      where i.Components == "foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("component = \"foo\"");
    }

    [Test]
    public void VersionsNotEqual()
    {
        var queryable = CreateQueryable();
        var issues = (from i in queryable
                      where i.FixVersions != "1.0" && i.AffectsVersions != "2.0"
                      select i).ToArray();

        _translator.Jql.Should().Be("(FixVersion != \"1.0\" and AffectedVersion != \"2.0\")");
    }

    [Test]
    public void ComponentNotEqual()
    {
        var queryable = CreateQueryable();
        var issues = (from i in queryable
                      where i.Components != "foo"
                      select i).ToArray();

        _translator.Jql.Should().Be("component != \"foo\"");
    }

    [Test]
    public void CanUseLiteralMatchOnMemberProperties()
    {
        var queryable = CreateQueryable();
        var issues = (from i in queryable
                      where i.Summary == new LiteralMatch("Literal Summary") && i.Description == new LiteralMatch("Literal Description")
                      select i).ToArray();

        _translator.Jql.Should().Be("(Summary = \"Literal Summary\" and Description = \"Literal Description\")");
    }

    [Test]
    public void MultipleSeparateWheres()
    {
        var queryable = CreateQueryable();

        var issues = from i in queryable
                     where i.Votes == 5
                     select i;

        issues = from i in issues
                 where i.Status == "Open" && i.Assignee == "admin"
                 select i;

        issues = from i in issues
                 where i.Priority == "1"
                 select i;

        var issuesArray = issues.ToArray();

        _translator.Jql.Should().Be("Votes = 5 and (Status = \"Open\" and Assignee = \"admin\") and Priority = \"1\"");
    }
}
