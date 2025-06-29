using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;

public class CustomHandlerProvider : IAuthenticationHandlerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthenticationScheme _scheme;

    public CustomHandlerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scheme = new AuthenticationScheme(
            TestAuthHandler.Scheme,
            TestAuthHandler.Scheme,
            typeof(TestAuthHandler));
    }

    public Task<IAuthenticationHandler?> GetHandlerAsync(HttpContext context, string authenticationScheme)
    {
        if (authenticationScheme == TestAuthHandler.Scheme)
        {
            var options = _serviceProvider.GetRequiredService<IOptionsMonitor<AuthenticationSchemeOptions>>();
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var encoder = _serviceProvider.GetRequiredService<UrlEncoder>();
            var clock = _serviceProvider.GetRequiredService<ISystemClock>();
            var mutator = _serviceProvider.GetRequiredService<Action<List<Claim>>>();

            var handler = new TestAuthHandler(options, loggerFactory, encoder, clock, mutator);
            handler.InitializeAsync(_scheme, context).GetAwaiter().GetResult();
            return Task.FromResult<IAuthenticationHandler?>(handler);
        }

        return Task.FromResult<IAuthenticationHandler?>(null);
    }
}