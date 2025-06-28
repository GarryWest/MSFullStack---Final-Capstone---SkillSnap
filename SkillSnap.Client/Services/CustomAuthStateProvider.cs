using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SkillSnap.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly UserStateService _userState;
    private const string TokenKey = "authToken";

    public CustomAuthStateProvider(IJSRuntime js, UserStateService userState)
    {
        _js = js;
        _userState = userState;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Console.WriteLine("[AuthStateProvider] Getting authentication state...");
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            _userState.Clear();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            _userState.Clear();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        // Console.WriteLine($"[AuthStateProvider] Token found, user authenticated: {user.Identity?.IsAuthenticated}");
        // Console.WriteLine($"[AuthStateProvider] Name: {user.Identity?.Name}");
        // Console.WriteLine($"[AuthStateProvider] Claims: {string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}"))}");



        _userState.SetUser(user);

        // Console.WriteLine($"[AuthStateProvider] Token found, user: {user.Identity?.Name}");

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        _userState.SetUser(user);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        _userState.Clear();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
}