using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "Test";
    private readonly Action<List<Claim>> _mutateClaims;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock,
        Action<List<Claim>> mutateClaims)
        : base(options, loggerFactory, encoder, clock)
    {
        _mutateClaims = mutateClaims;
        Console.WriteLine("TestAuthHandler: Initialized with custom claim mutator.");
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        _mutateClaims(claims); // 👈 Apply customizations

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        Logger.LogInformation("TestAuthHandler: Authenticated user with claims: {Claims}",
        string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));



        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}