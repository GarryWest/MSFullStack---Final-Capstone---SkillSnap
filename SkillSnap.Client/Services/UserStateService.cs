using System.Security.Claims;

namespace SkillSnap.Client.Services;

public class UserStateService
{
    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? Name { get; private set; }
    public List<string> Roles { get; private set; } = new();

    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);

    public void SetUser(ClaimsPrincipal user)
{
    // Console.WriteLine("[UserStateService] Setting user...");

    if (user.Identity?.IsAuthenticated == true)
    {
        UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Email = user.FindFirst(ClaimTypes.Email)?.Value;
        Name = user.Identity.Name ?? Email;
        Roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        // Console.WriteLine($"[UserStateService] Authenticated: true, Name: {Name}, Roles: {string.Join(", ", Roles)}");
    }
    else
    {
        // Console.WriteLine("[UserStateService] User not authenticated.");
        Clear();
    }
}

    public void Clear()
    {
        UserId = null;
        Email = null;
        Name = null;
        Roles.Clear();
    }
}