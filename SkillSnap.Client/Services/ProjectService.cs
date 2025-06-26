using System.Net.Http.Json;
using SkillSnap.Shared.Models;

public class ProjectService
{
    private readonly HttpClient _httpClient;

    public ProjectService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        var response = await _httpClient.GetAsync("api/projects");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Project>>();
    }

    public async Task<Project> AddProjectAsync(Project project)
    {
        var response = await _httpClient.PostAsJsonAsync("api/projects", project);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Project>();
    }
}