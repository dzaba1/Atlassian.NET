using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration;

public class ProjectTest
{
    private readonly Random _random = new Random();

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetIssueTypes(Jira jira)
    {
        var project = await jira.Projects.GetProjectAsync("TST");
        var issueTypes = await project.GetIssueTypesAsync().ToArrayAsync();

        Assert.True(issueTypes.Any());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task AddAndRemoveProjectComponent(Jira jira)
    {
        var componentName = "New Component " + _random.Next(int.MaxValue);
        var projectInfo = new ProjectComponentCreationInfo(componentName);
        var project = await jira.Projects.GetProjectsAsync().FirstAsync();

        // Add a project component.
        var component = await project.AddComponentAsync(projectInfo);
        Assert.Equal(componentName, component.Name);

        // Retrive project components.
        Assert.Contains(await project.GetComponentsAsync().ToArrayAsync(), p => p.Name == componentName);

        // Delete project component
        await project.DeleteComponentAsync(component.Name);
        Assert.DoesNotContain(await project.GetComponentsAsync().ToArrayAsync(), p => p.Name == componentName);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetProjectComponents(Jira jira)
    {
        var components = await jira.Components.GetComponentsAsync("TST").ToArrayAsync();
        Assert.Equal(2, components.Count());
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task GetAndUpdateProjectVersions(Jira jira)
    {
        var startDate = new DateTime(2000, 11, 1);
        var versions = await jira.Versions.GetVersionsAsync("TST").ToArrayAsync();
        Assert.True(versions.Count() >= 3);

        var version = versions.First(v => v.Name == "1.0");
        var newDescription = "1.0 Release " + _random.Next(int.MaxValue);
        version.Description = newDescription;
        version.StartDate = startDate;
        await version.SaveChangesAsync();

        Assert.Equal(newDescription, version.Description);
        version = await jira.Versions.GetVersionsAsync("TST").FirstAsync(v => v.Name == "1.0");
        Assert.Equal(newDescription, version.Description);
        Assert.Equal(version.StartDate, startDate);
    }

    [Theory]
    [ClassData(typeof(JiraProvider))]
    public async Task AddAndRemoveProjectVersions(Jira jira)
    {
        var versionName = "New Version " + _random.Next(int.MaxValue);
        var project = await jira.Projects.GetProjectsAsync().FirstAsync();
        var projectInfo = new ProjectVersionCreationInfo(versionName);
        projectInfo.StartDate = new DateTime(2000, 11, 1);

        // Add a project version.
        var version = await project.AddVersionAsync(projectInfo);
        Assert.Equal(versionName, version.Name);
        Assert.Equal(version.StartDate, projectInfo.StartDate);

        // Retrive project versions.
        Assert.Contains(await project.GetPagedVersionsAsync(), p => p.Name == versionName);

        // Delete project version
        await project.DeleteVersionAsync(version.Name);
        Assert.DoesNotContain(await project.GetPagedVersionsAsync(), p => p.Name == versionName);
    }
}
