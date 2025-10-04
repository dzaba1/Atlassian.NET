using Atlassian.Jira.Remote;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration;

public class JiraTypesTest
{
    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetFilters(Jira jira)
    {
        var filters = await jira.Filters.GetFavouritesAsync();

        Assert.True(filters.Count() >= 1);
        Assert.Contains(filters, f => f.Name == "One Issue Filter");

        var filter = await jira.Filters.GetFilterAsync(filters.First().Id);
        Assert.NotNull(filter);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task RetrieveNamedEntities(Jira jira)
    {
        var issue = await jira.Issues.GetIssueAsync("TST-1");

        Assert.Equal("Bug", issue.Type.Name);
        Assert.Equal("Major", issue.Priority.Name);
        Assert.Equal("Open", issue.Status.Name);
        Assert.Null(issue.Resolution);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueTypes(Jira jira)
    {
        var issueTypes = await jira.IssueTypes.GetIssueTypesAsync();

        // In addition, rest API contains "Sub-Task" as an issue type.
        Assert.True(issueTypes.Count() >= 5);
        Assert.Contains(issueTypes, i => i.Name == "Bug");
        Assert.NotNull(issueTypes.First().IconUrl);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssuePriorities(Jira jira)
    {
        var priorities = await jira.Priorities.GetPrioritiesAsync();

        Assert.Contains(priorities, i => i.Name == "Blocker");
        Assert.NotNull(priorities.First().IconUrl);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueResolutions(Jira jira)
    {
        var resolutions = await jira.Resolutions.GetResolutionsAsync();

        Assert.Contains(resolutions, i => i.Name == "Fixed");
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueStatuses(Jira jira)
    {
        var statuses = await jira.Statuses.GetStatusesAsync();

        var status = statuses.FirstOrDefault(i => i.Name == "Open");
        Assert.NotNull(status);
        Assert.NotNull(status.IconUrl);
        Assert.NotNull(status.StatusCategory);
        Assert.Equal("2", status.StatusCategory.Id);
        Assert.Equal("new", status.StatusCategory.Key);
        Assert.Equal("To Do", status.StatusCategory.Name);
        Assert.Equal("blue-gray", status.StatusCategory.ColorName);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueStatusById(Jira jira)
    {
        var status = await jira.Statuses.GetStatusAsync("1");

        Assert.NotNull(status);
        Assert.Equal("1", status.Id);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueStatusByName(Jira jira)
    {
        var status = await jira.Statuses.GetStatusAsync("Open");

        Assert.NotNull(status);
        Assert.Equal("Open", status.Name);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueStatusByInvalidNameShouldThrowException(Jira jira)
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await jira.Statuses.GetStatusAsync("InvalidName"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetCustomFields(Jira jira)
    {
        var fields = await jira.Fields.GetCustomFieldsAsync();
        Assert.True(fields.Count() >= 19);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetProjects(Jira jira)
    {
        var projects = await jira.Projects.GetProjectsAsync();
        Assert.True(projects.Count() > 0);

        var project = projects.First();
        Assert.Equal("admin", project.Lead);
        Assert.Equal("admin", project.LeadUser.DisplayName);
        Assert.NotNull(project.AvatarUrls);
        Assert.NotNull(project.AvatarUrls.XSmall);
        Assert.NotNull(project.AvatarUrls.Small);
        Assert.NotNull(project.AvatarUrls.Medium);
        Assert.NotNull(project.AvatarUrls.Large);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetProject(Jira jira)
    {
        var project = await jira.Projects.GetProjectAsync("TST");
        Assert.Equal("admin", project.Lead);
        Assert.Equal("admin", project.LeadUser.DisplayName);
        Assert.Equal("Test Project", project.Name);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetProjectStatusesAsync(Jira jira)
    {
        Predicate<IssueType> filter = x => x.Name == "Improvement" && x.Statuses.Any(s => s.Name == "Resolved");

        // Validate that issue types are returned with the valid statuses
        var issueTypes = await jira.IssueTypes.GetIssueTypesForProjectAsync("TST");
        Assert.Contains(issueTypes, filter);

        // Validate that different projects return different info
        issueTypes = await jira.IssueTypes.GetIssueTypesForProjectAsync("SCRUM");
        Assert.DoesNotContain(issueTypes, filter);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueLinkTypes(Jira jira)
    {
        var linkTypes = await jira.Links.GetLinkTypesAsync();
        Assert.Contains(linkTypes, l => l.Name.Equals("Duplicate"));
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueStatusesAsync(Jira jira)
    {
        // First request.
        var result1 = await jira.Statuses.GetStatusesAsync();
        Assert.NotEmpty(result1);

        // Cached
        var result2 = await jira.Statuses.GetStatusesAsync();
        Assert.Equal(result1.Count(), result2.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueTypesAsync(Jira jira)
    {
        // First request.
        var result1 = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
        Assert.NotEmpty(result1);

        // Cached
        var result2 = await jira.IssueTypes.GetIssueTypesAsync(CancellationToken.None);
        Assert.Equal(result1.Count(), result2.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssuePrioritiesAsync(Jira jira)
    {
        // First request.
        var result1 = await jira.Priorities.GetPrioritiesAsync();
        Assert.NotEmpty(result1);

        // Cached
        var result2 = await jira.Priorities.GetPrioritiesAsync();
        Assert.Equal(result1.Count(), result2.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueResolutionsAsync(Jira jira)
    {
        // First request.
        var result1 = await jira.Resolutions.GetResolutionsAsync();
        Assert.NotEmpty(result1);

        // Cached
        var result2 = await jira.Resolutions.GetResolutionsAsync();
        Assert.Equal(result1.Count(), result2.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetFavouriteFiltersAsync(Jira jira)
    {
        var result1 = await jira.Filters.GetFavouritesAsync();
        Assert.NotEmpty(result1);
    }
}
