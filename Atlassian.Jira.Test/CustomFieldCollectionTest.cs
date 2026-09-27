using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using FluentAssertions;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class CustomFieldCollectionTest
{
    [Test]
    public async Task IndexByName_ShouldThrowIfUnableToFindRemoteValue()
    {
        var jira = TestableJira.Create();
        jira.SetupIssues(new RemoteIssue() { key = "123" });

        var issue = new RemoteIssue()
        {
            project = "bar",
            key = "foo",
            customFieldValues = new RemoteCustomFieldValue[]{
                            new RemoteCustomFieldValue(){
                                customfieldId = "123",
                                values = new string[] {"abc"}
                            }
                        }
        }.ToLocal(jira);

        Func<Task> act = () => issue.GetCustomFieldAsync("CustomField");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task IndexByName_ShouldReturnRemoteValue()
    {
        //arrange
        var jira = TestableJira.Create();
        var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
        jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
            .Returns(Enumerable.Repeat<CustomField>(customField, 1).ToAsyncEnumerable());

        var issue = new RemoteIssue()
        {
            project = "projectKey",
            key = "issueKey",
            customFieldValues = new RemoteCustomFieldValue[]{
                            new RemoteCustomFieldValue(){
                                customfieldId = "123",
                                values = new string[] {"abc"}
                            }
                        }
        }.ToLocal(jira);

        //assert
        (await issue.GetCustomFieldAsync("CustomField")).Should().Be("abc");
        (await issue.CustomFields.GetCustomFieldAsync("CustomField")).Id.Should().Be("123");

        await issue.SetCustomFieldAsync("customfield", "foobar");
        (await issue.GetCustomFieldAsync("customfield")).Should().Be("foobar");
    }

    [Test]
    public async Task WillThrowErrorIfCustomFieldNotFound()
    {
        // Arrange
        var jira = TestableJira.Create();
        var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
        jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
            .Returns(Enumerable.Repeat<CustomField>(customField, 1).ToAsyncEnumerable());

        var issue = new RemoteIssue()
        {
            project = "projectKey",
            key = "issueKey",
            customFieldValues = null,
        }.ToLocal(jira);

        // Act / Assert
        Func<Task> act = async () => _ = (await issue.CustomFields.GetCustomFieldAsync("NonExistantField")).Values[0];
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
