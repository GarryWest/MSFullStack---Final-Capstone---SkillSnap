using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services;

public class SkillService
{
    private readonly HttpClient _httpClient;

    public SkillService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
        var response = await _httpClient.GetAsync("api/skills");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Skill>>();
    }

    public async Task<Skill> AddSkillAsync(Skill skill)
    {
        var response = await _httpClient.PostAsJsonAsync("api/skills", skill);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Skill>();
    }
}
