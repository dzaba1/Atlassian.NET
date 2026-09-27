using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class QueryParametersTest
{
    [Test]
    public void GetQueryParametersFromPath()
    {
        // Arrange
        var url = "?field1=9&field2=Test";

        // Act
        var parameters =  QueryParametersHelper.GetParametersFromPath(url);

        // Assert
        parameters.Should().NotBeNull();
        parameters.Count().Should().Be(2);

        parameters.First().Name.Should().Be("field1");
        parameters.First().Value.Should().Be("9");

        parameters.ElementAt(1).Name.Should().Be("field2");
        parameters.ElementAt(1).Value.Should().Be("Test");
    }

    [Test]
    public void GetQueryParametersFromPathNoEqual()
    {
        // Arrange
        var url = "?field1";

        // Act
        var parameters = QueryParametersHelper.GetParametersFromPath(url);

        // Assert
        parameters.Should().NotBeNull();
        parameters.Should().ContainSingle();

        parameters.First().Name.Should().Be("field1");
        parameters.First().Value.Should().Be("");
    }

    [Test]
    public void GetQueryParametersFromPathMultipleEquals()
    {
        // Arrange
        var url = "?field1=value=string==";

        // Act
        var parameters = QueryParametersHelper.GetParametersFromPath(url);

        // Assert
        parameters.Should().NotBeNull();
        parameters.Should().ContainSingle();

        parameters.First().Name.Should().Be("field1");
        parameters.First().Value.Should().Be("value=string==");
    }
}
