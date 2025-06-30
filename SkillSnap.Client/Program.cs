using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// auth + state
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// your JWT delegating handler
builder.Services.AddTransient<JwtAuthorizationMessageHandler>();

// ──────────────────────────────────────────────────────────────────────────────
// 1) Register ONE named HttpClient with BaseAddress + JWT handler
// ──────────────────────────────────────────────────────────────────────────────
const string ApiClientName = "SkillSnapApi";
builder.Services
  .AddHttpClient(ApiClientName, client =>
  {
      client.BaseAddress = new Uri("http://localhost:5072/");
  })
  .AddHttpMessageHandler<JwtAuthorizationMessageHandler>()

  // ────────────────────────────────────────────────────────────────────────────
  // 2) “Attach” your typed clients to that single pipeline
  // ────────────────────────────────────────────────────────────────────────────
  .AddTypedClient<AuthService>()
  .AddTypedClient<IProjectService, ProjectService>()
  .AddTypedClient<SkillService>()
  .AddTypedClient<IPortfolioUserService, PortfolioUserService>()
  ;

// ──────────────────────────────────────────────────────────────────────────────
// 3) Any other scoped/transient/non-HTTP services
// ──────────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<UserStateService>();

await builder.Build().RunAsync();