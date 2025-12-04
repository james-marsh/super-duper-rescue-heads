using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SuperDuperRescueHeads.Web.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ProtectedSessionStorage _sessionStorage;

    public AuthTokenHandler(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>("authToken");
            var token = result.Success ? result.Value : null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // If session storage isn't available yet, continue without token
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
