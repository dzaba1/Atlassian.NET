using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using Xunit;

namespace Atlassian.Jira.Test;

public class CustomFieldCollectionTest
{
    [Fact]
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => issue.GetCustomFieldAsync("CustomField"));
    }

    [Fact]
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
        Assert.Equal("abc", await issue.GetCustomFieldAsync("CustomField"));
        Assert.Equal("123", (await issue.CustomFields.GetCustomFieldAsync("CustomField")).Id);

        await issue.SetCustomFieldAsync("customfield", "foobar");
        Assert.Equal("foobar", await issue.GetCustomFieldAsync("customfield"));
    }

    [Fact]
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
        await Assert.ThrowsAsync<InvalidOperationException>(async () => _ = (await issue.CustomFields.GetCustomFieldAsync("NonExistantField")).Values[0]);
    }
}
