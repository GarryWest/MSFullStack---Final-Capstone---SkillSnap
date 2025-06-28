using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.JSInterop;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private const string TokenKey = "authToken";
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(HttpClient http, IJSRuntime js, CustomAuthStateProvider authStateProvider)
    {
        _http = http;
        _js = js;
        _authStateProvider = authStateProvider;
    }


    public async Task<bool> LoginAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<JwtResponse>();
        if (result is null) return false;

        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Token);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        _authStateProvider.NotifyUserAuthentication(result.Token);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _http.DefaultRequestHeaders.Authorization = null;
        _authStateProvider.NotifyUserLogout();
    }

    public async Task<bool> RegisterAsync(RegisterModel model)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", model);
        return response.IsSuccessStatusCode;
    }

    public async Task InitializeAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private class JwtResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    public bool IsAuthenticated()
    {
        return _http.DefaultRequestHeaders.Authorization != null;
    }

    public async Task<string?> GetUserEmailAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        if (string.IsNullOrWhiteSpace(token)) return null;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
    }

    public async Task<T?> HandleApiCall<T>(Func<Task<HttpResponseMessage>> apiCall, Action? onUnauthorized = null)
    {
        try
        {
            var response = await apiCall();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                onUnauthorized?.Invoke();
                return default;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
            return default;
        }
    }
}