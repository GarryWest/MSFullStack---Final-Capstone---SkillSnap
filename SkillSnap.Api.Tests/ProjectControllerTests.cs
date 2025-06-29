using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using SkillSnap.Shared.Models;
using SkillSnap.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit.Abstractions;

public class ProjectsControllerTests
{
    [Fact]
    public async Task AddProject_ReturnsForbiddenForNonAdmin()
    {
        // Simulate a user without Admin role
        // This assumes your CustomWebApplicationFactory can modify user claims
        var factory = new CustomWebApplicationFactory(user =>
        {
            user.RemoveAll(c => c.Type == ClaimTypes.Role); // Remove Admin role
        });

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/projects", new Project
        {
            Title = "Unauthorized Project",
            Description = "Should not be allowed"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanPostProject()
    {
        var client = CreateClientWithRole("Admin");

        var newProject = new Project
        {
            Title = "Admin Project",
            Description = "Created by admin",
            PortfolioUserId = 1
        };

        var response = await client.PostAsJsonAsync("/api/projects", newProject);
        //response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}, Body: {content}");

        Assert.Contains("Admin Project", content);
    }

    [Fact]
    public async Task AddProject_UpdatesProjectList()
    {
        var client = CreateClientWithRole("Admin");

        var newProject = new Project
        {
            Title = "New Project",
            Description = "Added via test",
            PortfolioUserId = 1
        };

        var postResponse = await client.PostAsJsonAsync("/api/projects", newProject);
        postResponse.EnsureSuccessStatusCode();

        var projects = await client.GetFromJsonAsync<List<ProjectDto>>("/api/projects");
        Assert.Contains(projects, p => p.Title == "New Project");
    }

    [Fact]
    public async Task GetMine_DoesNotReturnOtherUsersProjects()
    {
        // Seed a second user and project
        var factory = new CustomWebApplicationFactory(user =>
        {
            user.RemoveAll(c => c.Type == ClaimTypes.Role); // Remove Admin role
        });
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SkillSnapContext>();

        var otherUser = new PortfolioUser { Id = 2, Name = "Other User", ApplicationUserId = "other-user-id" };
        context.PortfolioUsers.Add(otherUser);
        context.Projects.Add(new Project { Title = "Other Project", PortfolioUserId = otherUser.Id });
        context.SaveChanges();

        // Act
        var client = CreateClientWithRole("User");
        var projects = await client.GetFromJsonAsync<List<ProjectDto>>("/api/projects/mine");

        // Assert
        Assert.DoesNotContain(projects, p => p.Title == "Other Project");
    }


    [Fact]
    public async Task GetMine_ReturnsCorrectProjectDto()
    {
        var client = CreateClientWithRole("User");
        var projects = await client.GetFromJsonAsync<List<ProjectDto>>("/api/projects/mine");

        Assert.NotNull(projects);
        Assert.Single(projects);
        Assert.Equal("User Project", projects[0].Title);
        Assert.Equal("Test User", projects[0].PortfolioUserName);
    }

    [Fact(Skip = "Comment out SkillSnap.Api/Program.cs call to DatabaseSeeder.SeedAsync() to run this test")]
    public async Task GetMine_ReturnsUnauthorizedWithoutAuth()
    {
        // Simulate a user without authentication
        var factory = new UnauthenticatedWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/projects/mine");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_ReturnsUserProjects()
    {
        var client = CreateClientWithRole("User");

        var response = await client.GetAsync("/api/projects/mine");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("User Project", content);
    }

    [Fact(Skip = "Comment out SkillSnap.Api/Program.cs call to DatabaseSeeder.SeedAsync() to run this test")]
    public async Task UnauthenticatedUser_CannotPostProject()
    {
        var factory = new UnauthenticatedWebApplicationFactory();
        var client = factory.CreateClient();

        var newProject = new Project
        {
            Title = "Unauthorized Project",
            Description = "Should not be allowed",
            PortfolioUserId = 1
        };

        var response = await client.PostAsJsonAsync("/api/projects", newProject);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateClientWithRole(string role)
    {
        var factory = new CustomWebApplicationFactory(claims =>
        {
            claims.RemoveAll(c => c.Type == ClaimTypes.Role);
            claims.Add(new Claim(ClaimTypes.Role, role));
        });

        return factory.CreateClient();
    }

    private HttpClient CreateUnauthenticatedClient()
    {
        var factory = new CustomWebApplicationFactory(claims =>
        {
            // No claims = unauthenticated
        });

        return factory.CreateClient();
    }

}