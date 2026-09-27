using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class IssueTest
{
    public class Constructor
    {
        [Test]
        public void ShouldSetDefaultValues()
        {
            var issue = CreateIssue("ProjectKey");
            issue.AffectsVersions.Should().BeEmpty();
            issue.Assignee.Should().BeNull();
            issue.Components.Should().BeEmpty();
            issue.Created.Should().BeNull();
            issue.CustomFields.Should().BeEmpty();
            issue.Description.Should().BeNull();
            issue.DueDate.Should().BeNull();
            issue.Environment.Should().BeNull();
            issue.Key.Should().BeNull();
            issue.Priority.Should().BeNull();
            issue.Project.Should().Be("ProjectKey");
            issue.Reporter.Should().BeNull();
            issue.Resolution.Should().BeNull();
            issue.Status.Should().BeNull();
            issue.Summary.Should().BeNull();
            issue.Type.Should().BeNull();
            issue.Updated.Should().BeNull();
            issue.Votes.Should().BeNull();
        }

        [Test]
        public void FromRemote_ShouldPopulateFields()
        {
            var remoteIssue = new RemoteIssue()
            {
                affectsVersions = new RemoteVersion[] { new RemoteVersion() { id = "remoteVersion" } },
                assignee = "assignee",
                components = new RemoteComponent[] { new RemoteComponent() { id = "remoteComponent" } },
                created = new DateTime(2011, 1, 1),
                customFieldValues = new RemoteCustomFieldValue[] { new RemoteCustomFieldValue() { customfieldId = "customField" } },
                description = "description",
                duedate = new DateTime(2011, 3, 3),
                environment = "environment",
                fixVersions = new RemoteVersion[] { new RemoteVersion() { id = "remoteFixVersion" } },
                key = "key",
                priority = new RemotePriority() { id = "priority" },
                project = "project",
                reporter = "reporter",
                resolution = new RemoteResolution() { id = "resolution" },
                status = new RemoteStatus() { id = "status" },
                summary = "summary",
                type = new RemoteIssueType() { id = "type" },
                updated = new DateTime(2011, 2, 2),
                votesData = new RemoteVotes() { votes = 1, hasVoted = true }
            };

            var issue = remoteIssue.ToLocal(TestableJira.Create());

            issue.AffectsVersions.Should().ContainSingle();
            issue.Assignee.Should().Be("assignee");
            issue.Components.Should().ContainSingle();
            issue.Created.Should().Be(new DateTime(2011, 1, 1));
            issue.CustomFields.Should().ContainSingle();
            issue.Description.Should().Be("description");
            issue.DueDate.Should().Be(new DateTime(2011, 3, 3));
            issue.Environment.Should().Be("environment");
            issue.Key.Value.Should().Be("key");
            issue.Priority.Id.Should().Be("priority");
            issue.Project.Should().Be("project");
            issue.Reporter.Should().Be("reporter");
            issue.Resolution.Id.Should().Be("resolution");
            issue.Status.Id.Should().Be("status");
            issue.Summary.Should().Be("summary");
            issue.Type.Id.Should().Be("type");
            issue.Updated.Should().Be(new DateTime(2011, 2, 2));
            issue.Votes.Should().Be(1);
            issue.HasUserVoted.Should().BeTrue();
        }
    }

    public class ToRemote
    {
        [Test]
        public async Task IfFieldsNotSet_ShouldLeaveFieldsNull()
        {
            var issue = CreateIssue("ProjectKey");

            var remoteIssue = await issue.ToRemoteAsync();

            remoteIssue.affectsVersions.Should().BeNull();
            remoteIssue.assignee.Should().BeNull();
            remoteIssue.components.Should().BeNull();
            remoteIssue.created.Should().BeNull();
            remoteIssue.customFieldValues.Should().BeNull();
            remoteIssue.description.Should().BeNull();
            remoteIssue.duedate.Should().BeNull();
            remoteIssue.environment.Should().BeNull();
            remoteIssue.key.Should().BeNull();
            remoteIssue.priority.Should().BeNull();
            remoteIssue.project.Should().Be("ProjectKey");
            remoteIssue.reporter.Should().BeNull();
            remoteIssue.resolution.Should().BeNull();
            remoteIssue.status.Should().BeNull();
            remoteIssue.summary.Should().BeNull();
            remoteIssue.type.Should().BeNull();
            remoteIssue.updated.Should().BeNull();
            remoteIssue.votesData.Should().BeNull();
        }

        [Test]
        public async Task IfFieldsSet_ShouldPopulateFields()
        {
            var jira = TestableJira.Create();
            var issue = jira.CreateIssue("ProjectKey");
            var version = new RemoteVersion() { id = "1" }.ToLocal(issue.Jira);
            var component = new RemoteComponent() { id = "1" }.ToLocal();

            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssueType("4", "issuetype"), 1).ToAsyncEnumerable());
            jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssuePriority("1", "priority"), 1).ToAsyncEnumerable());

            issue.AffectsVersions.Add(version);
            issue.Assignee = "assignee";
            issue.Components.Add(component);
            // issue.CustomFields <-- requires extra setup, test below
            issue.Description = "description";
            issue.DueDate = new DateTime(2011, 1, 1);
            issue.Environment = "environment";
            issue.FixVersions.Add(version);
            // issue.Key <-- should be non-settable
            issue.Priority = "1";
            // issue.Project <-- should be non-settable
            issue.Reporter = "reporter";
            issue.Summary = "summary";
            issue.Type = "4";

            var remoteIssue = await issue.ToRemoteAsync();

            remoteIssue.affectsVersions.Should().ContainSingle();
            remoteIssue.assignee.Should().Be("assignee");
            remoteIssue.components.Should().ContainSingle();
            remoteIssue.created.Should().BeNull();
            remoteIssue.description.Should().Be("description");
            remoteIssue.duedate.Should().Be(new DateTime(2011, 1, 1));
            remoteIssue.environment.Should().Be("environment");
            remoteIssue.key.Should().BeNull();
            remoteIssue.priority.id.Should().Be("1");
            remoteIssue.project.Should().Be("ProjectKey");
            remoteIssue.reporter.Should().Be("reporter");
            remoteIssue.resolution.Should().BeNull();
            remoteIssue.status.Should().BeNull();
            remoteIssue.summary.Should().Be("summary");
            remoteIssue.type.id.Should().Be("4");
            remoteIssue.updated.Should().BeNull();
        }

        [Test]
        public async Task ToRemote_IfTypeSetByName_FetchId()
        {
            var jira = TestableJira.Create();
            var issue = jira.CreateIssue("ProjectKey");
            var issueType = new IssueType(new RemoteIssueType() { id = "1", name = "Bug" });
            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat<IssueType>(issueType, 1).ToAsyncEnumerable());

            issue.Type = "Bug";

            var remoteIssue = await issue.ToRemoteAsync();
            remoteIssue.type.id.Should().Be("1");
        }
    }

    public class GetUpdatedFields
    {
        [Test]
        public async Task ReturnsCustomFieldsAdded()
        {
            var jira = TestableJira.Create();
            var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
            var remoteIssue = new RemoteIssue()
            {
                key = "TST-1",
                project = "TST",
                type = new RemoteIssueType() { id = "1" }
            };

            jira.IssueService.SetupIssues(jira, remoteIssue);
            jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat<CustomField>(customField, 1).ToAsyncEnumerable());

            var issue = jira.CreateIssue("TST");
            await issue.SetCustomFieldAsync("My Custom Field", "test value");

            var result = await GetUpdatedFieldsForIssueAsync(issue);
            result.Should().ContainSingle();
            result.First().id.Should().Be("CustomField1");
        }

        [Test]
        public async Task ExcludesCustomFieldsNotModified()
        {
            var jira = TestableJira.Create();
            var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
            var remoteCustomFieldValue = new RemoteCustomFieldValue()
            {
                customfieldId = "CustomField1",
                values = new string[1] { "My Value" }
            };
            var remoteIssue = new RemoteIssue()
            {
                key = "TST-1",
                project = "TST",
                type = new RemoteIssueType() { id = "1" },
                customFieldValues = new RemoteCustomFieldValue[1] { remoteCustomFieldValue }
            };

            jira.IssueService.Setup(s => s.GetIssueAsync("TST-1", CancellationToken.None))
                .Returns(Task.FromResult(new Issue(jira, remoteIssue)));
            jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat<CustomField>(customField, 1).ToAsyncEnumerable());
            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssueType("1"), 1).ToAsyncEnumerable());

            var issue = await jira.Issues.GetIssueAsync("TST-1");

            var result = await GetUpdatedFieldsForIssueAsync(issue);
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ReturnsCustomFieldThatWasModified()
        {
            var jira = TestableJira.Create();
            var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
            var remoteCustomFieldValue = new RemoteCustomFieldValue()
            {
                customfieldId = "CustomField1",
                values = new string[1] { "My Value" }
            };
            var remoteIssue = new RemoteIssue()
            {
                key = "TST-1",
                project = "TST",
                type = new RemoteIssueType() { id = "1" },
                customFieldValues = new RemoteCustomFieldValue[1] { remoteCustomFieldValue }
            };

            jira.IssueService.Setup(s => s.GetIssueAsync("TST-1", CancellationToken.None))
                .Returns(Task.FromResult(new Issue(jira, remoteIssue)));
            jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat<CustomField>(customField, 1).ToAsyncEnumerable());
            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssueType("1"), 1).ToAsyncEnumerable());

            var issue = await jira.Issues.GetIssueAsync("TST-1");
            await issue.SetCustomFieldAsync("My Custom Field", "My New Value");

            var result = await GetUpdatedFieldsForIssueAsync(issue);
            result.Should().ContainSingle();
            result.First().id.Should().Be("CustomField1");
            result.First().values[0].Should().Be("My New Value");
        }

        [Test]
        public async Task IfIssueTypeWithId_ReturnField()
        {
            var jira = TestableJira.Create();
            var issue = jira.CreateIssue("TST");
            issue.Priority = "5";

            jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssuePriority("5"), 1).ToAsyncEnumerable());

            var result = await GetUpdatedFieldsForIssueAsync(issue);
            result.Should().ContainSingle();
            result[0].values[0].Should().Be("5");
        }

        [Test]
        public async Task IfIssueTypeWithName_ReturnsFieldWithIdInferred()
        {
            var jira = TestableJira.Create();
            var issueType = new IssueType(new RemoteIssueType() { id = "2", name = "Task" });
            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat<IssueType>(issueType, 1).ToAsyncEnumerable());
            var issue = jira.CreateIssue("FOO");
            issue.Type = "Task";

            var result = await GetUpdatedFieldsForIssueAsync(issue);
            result.Should().ContainSingle();
            result[0].values[0].Should().Be("2");
        }

        [Test]
        public async Task IfIssueTypeWithNameNotChanged_ReturnsNoFieldsChanged()
        {
            var jira = TestableJira.Create();
            var issueType = new IssueType(new RemoteIssueType() { id = "5", name = "Task" });
            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(issueType, 1).ToAsyncEnumerable());
            var remoteIssue = new RemoteIssue()
            {
                type = new RemoteIssueType() { id = "5" },
            };

            var issue = remoteIssue.ToLocal(jira);
            issue.Type = "Task";

            var fields = await GetUpdatedFieldsForIssueAsync(issue);
            fields.Should().BeEmpty();
        }

        [Test]
        public async Task ReturnEmptyIfNothingChanged()
        {
            var issue = CreateIssue();

            (await GetUpdatedFieldsForIssueAsync(issue)).Should().BeEmpty();
        }

        [Test]
        public async Task IfString_ReturnOneFieldThatChanged()
        {
            var issue = CreateIssue();
            issue.Summary = "foo";

            (await GetUpdatedFieldsForIssueAsync(issue)).Should().ContainSingle();
        }

        [Test]
        public async Task IfString_ReturnAllFieldsThatChanged()
        {
            var jira = TestableJira.Create();
            var issue = jira.CreateIssue("TST");
            issue.Summary = "foo";
            issue.Description = "foo";
            issue.Assignee = "foo";
            issue.Environment = "foo";
            issue.Reporter = "foo";
            issue.Type = "2";
            issue.Resolution = "3";
            issue.Priority = "4";

            jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssuePriority("4"), 1).ToAsyncEnumerable());
            jira.IssueResolutionService.Setup(s => s.GetResolutionsAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssueResolution("3"), 1).ToAsyncEnumerable());
            jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssueType("2"), 1).ToAsyncEnumerable());

            (await GetUpdatedFieldsForIssueAsync(issue)).Length.Should().Be(8);
        }

        [Test]
        public async Task IfStringEqual_ReturnNoFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                summary = "Summary"
            };

            var issue = remoteIssue.ToLocal(TestableJira.Create());

            issue.Summary = "Summary";

            (await GetUpdatedFieldsForIssueAsync(issue)).Should().BeEmpty();
        }

        [Test]
        public async Task IfComparableEqual_ReturnNoFieldsThatChanged()
        {
            var jira = TestableJira.Create();
            var remoteIssue = new RemoteIssue()
            {
                priority = new RemotePriority() { id = "5" },
            };

            var issue = remoteIssue.ToLocal(jira);
            issue.Priority = "5";

            jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssuePriority("5"), 1).ToAsyncEnumerable());
            (await GetUpdatedFieldsForIssueAsync(issue)).Should().BeEmpty();
        }

        [Test]
        public async Task IfComparable_ReturnsFieldsThatChanged()
        {
            var jira = TestableJira.Create();
            var issue = jira.CreateIssue("TST");
            issue.Priority = "5";

            jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                .Returns(Enumerable.Repeat(new IssuePriority("5"), 1).ToAsyncEnumerable());

            (await GetUpdatedFieldsForIssueAsync(issue)).Should().ContainSingle();
        }

        [Test]
        public async Task IfDateTimeChanged_ReturnsFieldsThatChanged()
        {
            var issue = CreateIssue();
            issue.DueDate = new DateTime(2011, 10, 10);

            var fields = await GetUpdatedFieldsForIssueAsync(issue);
            fields.Should().ContainSingle();
            fields[0].values[0].Should().Be("10/Oct/11");
        }

        [Test]
        public async Task IfDateTimeUnChangd_ShouldNotIncludeItInFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                duedate = new DateTime(2011, 1, 1)
            };

            var issue = remoteIssue.ToLocal(TestableJira.Create());
            (await GetUpdatedFieldsForIssueAsync(issue)).Should().BeEmpty();
        }

        [Test]
        public async Task IfComponentsAdded_ReturnsFields()
        {
            var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
            var component = new RemoteComponent() { id = "1", name = "1.0" };
            issue.Components.Add(component.ToLocal());

            var fields = await GetUpdatedFieldsForIssueAsync(issue);
            fields.Should().ContainSingle();
            fields[0].id.Should().Be("components");
            fields[0].values[0].Should().Be("1");
        }

        [Test]
        public async Task IfAddFixVersion_ReturnAllFieldsThatChanged()
        {
            var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
            var version = new RemoteVersion() { id = "1", name = "1.0" };
            issue.FixVersions.Add(version.ToLocal(TestableJira.Create()));

            var fields = await GetUpdatedFieldsForIssueAsync(issue);
            fields.Should().ContainSingle();
            fields[0].id.Should().Be("fixVersions");
            fields[0].values[0].Should().Be("1");
        }

        [Test]
        public async Task IfAddAffectsVersion_ReturnAllFieldsThatChanged()
        {
            var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
            var version = new RemoteVersion() { id = "1", name = "1.0" };
            issue.AffectsVersions.Add(version.ToLocal(TestableJira.Create()));

            var fields = await GetUpdatedFieldsForIssueAsync(issue);
            fields.Should().ContainSingle();
            fields[0].id.Should().Be("versions");
            fields[0].values[0].Should().Be("1");
        }
    }

    public class GetAttachments
    {
        [Test]
        public async Task IfIssueNotCreated_ShouldThrowException()
        {
            var issue = CreateIssue();

            Func<Task> act = async () => await issue.GetAttachmentsAsync().ToArrayAsync();
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task IfIssueIsCreated_ShouldLoadAttachments()
        {
            //arrange
            var jira = TestableJira.Create();
            var remoteAttachment = new RemoteAttachment() { filename = "attach.txt" };
            jira.IssueService.Setup(j => j.GetAttachmentsAsync("issueKey", It.IsAny<CancellationToken>()))
                .Returns(Enumerable.Repeat<Attachment>(new Attachment(jira, remoteAttachment), 1).ToAsyncEnumerable());

            var issue = (new RemoteIssue() { key = "issueKey" }).ToLocal(jira);

            //act
            var attachments = await issue.GetAttachmentsAsync()
                .ToArrayAsync();

            //assert
            attachments.Should().ContainSingle();
            attachments.First().FileName.Should().Be("attach.txt");
        }
    }

    public class AddAttachment
    {
        [Test]
        public async Task AddAttachment_IfIssueNotCreated_ShouldThrowAnException()
        {
            var issue = CreateIssue();

            Func<Task> act = () => issue.AddAttachmentAsync("foo", new byte[] { 1 });
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    public class WorkflowTransition
    {
        [Test]
        public void IfTransitionNotFound_ShouldThrowAnException()
        {
            var jira = TestableJira.Create();
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            Action act = () => issue.WorkflowTransitionAsync("foo").Wait();
            act.Should().Throw<AggregateException>();
        }
    }

    public class GetComments
    {
        [Test]
        public async Task IfIssueNotCreated_ShouldThrowException()
        {
            var issue = CreateIssue();

            Func<Task> act = async () => await issue.GetCommentsAsync().ToArrayAsync();
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task IfIssueIsCreated_ShouldLoadComments()
        {
            //arrange
            var jira = TestableJira.Create();
            jira.IssueService.Setup(j => j.GetCommentsAsync("issueKey", It.IsAny<CancellationToken>()))
                .Returns(Enumerable.Repeat<Comment>(new Comment() { Body = "the comment" }, 1).ToAsyncEnumerable());
            var issue = (new RemoteIssue() { key = "issueKey" }).ToLocal(jira);

            //act
            var comments = await issue.GetCommentsAsync()
                .ToArrayAsync();

            //assert
            comments.Should().ContainSingle();
            comments.First().Body.Should().Be("the comment");
        }
    }

    private static Issue CreateIssue(string project = "TST")
    {
        return TestableJira.Create().CreateIssue(project);
    }

    private static Task<RemoteFieldValue[]> GetUpdatedFieldsForIssueAsync(Issue issue)
    {
        return ((IRemoteIssueFieldProvider)issue).GetRemoteFieldValuesAsync(CancellationToken.None);
    }
}
