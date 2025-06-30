using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services;

public class PortfolioUserService : IPortfolioUserService
{
    private readonly HttpClient _httpClient;

    public event Action? OnUnauthorized;

    public PortfolioUserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PortfolioUserDto?> GetPortfolioUserMineAsync()
    {
        try
        {
            var requestUri = "api/PortfolioUser/mine";
            Console.WriteLine($"[PortfolioUserService] Sending GET to: {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching portfolio user mine: {ex.Message}");
            return null;
        }
    }

    public async Task<PortfolioUserDto?> GetPortfolioUserAsync(string username)
    {
        try
        {
            var requestUri = $"api/PortfolioUser/{username}";
            Console.WriteLine($"[PortfolioUserService] Sending GET to: {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching portfolio user: {ex.Message}");
            return null;
        }
    }

    public async Task<List<PortfolioUserDto>?> GetPortfolioUsersAsync()
    {
        try
        {
            var requestUri = "api/PortfolioUser";
            Console.WriteLine($"[PortfolioUserService] Sending GET to: {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<PortfolioUserDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching portfolio users: {ex.Message}");
            return null;
        }
    }

    public async Task<PortfolioUserDto?> UpdatePortfolioUserAsync(PortfolioUserDto portfolioUser)
    {
        try
        {
            var requestUri = "api/PortfolioUser/update";
            Console.WriteLine($"[PortfolioUserService] Sending POST to: {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.PostAsJsonAsync(requestUri, portfolioUser);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating portfolio" +
                              $" user: {ex.Message}");
            return null;
        }
    }
}
