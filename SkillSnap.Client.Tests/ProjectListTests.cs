using Bunit;
using SkillSnap.Client.Pages;
using SkillSnap.Client.Services;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace SkillSnap.Client.Tests;

public class ProjectListTests : TestContext
{
    [Fact]
    public void ProjectList_RendersCorrectly()
    {
        // Arrange & Act
        var mockService = new Mock<IProjectService>();
        mockService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(new List<ProjectDto> {
            new ProjectDto
            {
                Id = 1,
                Title = "Test Project",
                Description = "This is a test project",
                PortfolioUserName = "Test User",
            },
            new ProjectDto
            {
                Id = 2,
                Title = "Another Project",
                Description = "This is another test project",
                PortfolioUserName = "Test User 2",
            },
            new ProjectDto
            {
                Id = 3,
                Title = "Third Project",
                Description = "This is the third test project",
                PortfolioUserName = "Test User 3"
            }
        });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ProjectList>();

        // Assert
        cut.MarkupMatches(@"<h3 class=""mb-4"">Projects</h3><p>Loading projects...</p>");
    }

}