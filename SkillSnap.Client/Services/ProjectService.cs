using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services;

public class ProjectService : IProjectService
{
    private readonly HttpClient _httpClient;

    public event Action? OnUnauthorized;

    public ProjectService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Console.WriteLine($"[ProjectService] HttpClient BaseAddress set to: {_httpClient.BaseAddress}");
    }

    public async Task<List<ProjectDto>?> GetProjectsAsync()
    {
        try
        {
            var requestUri = "api/projects";
            Console.WriteLine($"[ProjectService] Sending GET to: {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching projects: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ProjectDto>?> GetProjectsMineAsync()
    {
        try
        {
            var requestUri = "api/projects/mine";
            Console.WriteLine($"[ProjectService] Sending GET to: {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching projects: {ex.Message}");
            return null;
        }
    }

    public async Task<Project?> AddProjectAsync(Project project)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/projects", project);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding project: {ex.Message}");
            return null;
        }
    }
}