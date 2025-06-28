namespace SkillSnap.Client.Services;

using System.Net.Http.Headers;
using Microsoft.JSInterop;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "authToken";

    public JwtAuthorizationMessageHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}