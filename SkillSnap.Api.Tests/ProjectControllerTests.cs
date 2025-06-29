using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SkillSnap.Api.Tests;
using System.Net.Http.Json;
using SkillSnap.Shared.Models;


public class ProjectsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetMine_ReturnsUserProjects()
    {
        var response = await _client.GetAsync("/api/projects/mine");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("User Project", content);
    }

    [Fact]
    public async Task GetMine_ReturnsCorrectProjectDto()
    {
        var projects = await _client.GetFromJsonAsync<List<ProjectDto>>("/api/projects/mine");

        Assert.NotNull(projects);
        Assert.Single(projects);
        Assert.Equal("User Project", projects[0].Title);
        Assert.Equal("Test User", projects[0].PortfolioUserName);
    }

    [Fact(Skip = "Requires authentication")]
    public async Task GetMine_ReturnsOkForAuthenticatedUser()
    {
        var response = await _client.GetAsync("/api/projects/mine");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Requires authentication")]
    public async Task GetAll_ReturnsProjects()
    {
        var response = await _client.GetAsync("/api/projects");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Project", content);
    }
}