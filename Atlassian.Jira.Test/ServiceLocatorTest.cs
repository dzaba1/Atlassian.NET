using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using Moq;
using Xunit;

namespace Atlassian.Jira.Test;

public class ServiceLocatorTest
{
    [Fact]
    public async Task UserCanProvideCustomProjectVersionService()
    {
        // Arrange
        var projects = new Mock<IProjectService>();
        var client = new Mock<IJiraRestClient>();
        var jira = Jira.CreateRestClient(client.Object);

        var remoteProject = new RemoteProject() { id = "projId", key = "projKey", name = "my project" };
        projects.Setup(s => s.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .Returns(Enumerable.Repeat(new Project(jira, remoteProject), 1).ToAsyncEnumerable());
        jira.Services.Register<IProjectService>(() => projects.Object);

        var versionResource = new Mock<IProjectVersionService>();
        var remoteVersion = new RemoteVersion() { id = "123", name = "my version" };
        var version = new ProjectVersion(jira, remoteVersion);
        versionResource.Setup(s => s.GetVersionsAsync("projKey", CancellationToken.None))
            .Returns(Enumerable.Repeat<ProjectVersion>(version, 1).ToAsyncEnumerable());

        jira.Services.Register<IProjectVersionService>(() => versionResource.Object);

        // Act
        var proj = await jira.Projects.GetProjectsAsync()
            .FirstAsync();
        var versions = await proj.GetVersionsAsync().ToArrayAsync();

        // Assert
        Assert.Equal("my version", versions.First().Name);
    }

    [Fact]
    public async Task UserCanProvideCustomProjectComponentsService()
    {
        // Arrange
        var projects = new Mock<IProjectService>();
        var client = new Mock<IJiraRestClient>();
        var jira = Jira.CreateRestClient(client.Object);

        var remoteProject = new RemoteProject() { id = "projId", key = "projKey", name = "my project" };
        projects.Setup(s => s.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .Returns(Enumerable.Repeat(new Project(jira, remoteProject), 1).ToAsyncEnumerable());
        jira.Services.Register<IProjectService>(() => projects.Object);

        var componentResource = new Mock<IProjectComponentService>();
        var remoteComponent = new RemoteComponent() { id = "123", name = "my component" };
        var component = new ProjectComponent(remoteComponent);
        componentResource.Setup(s => s.GetComponentsAsync("projKey", CancellationToken.None))
            .Returns(Enumerable.Repeat<ProjectComponent>(component, 1).ToAsyncEnumerable());

        jira.Services.Register<IProjectComponentService>(() => componentResource.Object);

        // Act
        var proj = await jira.Projects.GetProjectsAsync()
            .FirstAsync();
        var components = await proj.GetComponentsAsync().ToArrayAsync();

        // Assert
        Assert.Equal("my component", components.First().Name);
    }
}
