using Atlassian.Jira.Remote;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class GreenhopperSprintCustomFieldValueSerialiserTest
{
    [Test]
    public void Test_FromJson()
    {
        var serialiser = new GreenhopperSprintCustomFieldValueSerialiser("name");

        var actual = serialiser.FromJson(@"
[
'com.atlassian.greenhopper.service.sprint.Sprint@e654c1[id=1,rapidViewId=1,state=FUTURE,name=Sprint1,startDate=<null>,endDate=<null>,completeDate=<null>,sequence=1',
'com.atlassian.greenhopper.service.sprint.Sprint@e654c1[id=2,rapidViewId=1,state=FUTURE,name=Sprint2,startDate=<null>,endDate=<null>,completeDate=<null>,sequence=2',
]
            ".Replace('\'', '\"'));

        var expected = new[] {"Sprint1", "Sprint2"};
        actual.Should().Equal(expected);
    }

    [Test]
    public void Test_ToJson()
    {
        var serialiser = new GreenhopperSprintCustomFieldValueSerialiser("name");

        var actual = serialiser.ToJson(new[]
        {
            "Sprint1",
            "Sprint2",
        });

        var expected = (JToken) "Sprint1";
        actual.ToString().Should().Be(expected.ToString());
    }
}
