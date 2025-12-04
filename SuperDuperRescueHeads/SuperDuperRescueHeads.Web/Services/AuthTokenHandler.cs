using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace SuperDuperRescueHeads.Web.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<AuthTokenHandler> _logger;

    public AuthTokenHandler(ProtectedSessionStorage sessionStorage, ILogger<AuthTokenHandler> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
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
        catch (Exception ex)
        {
            // If session storage isn't available yet (e.g., during prerendering), continue without token
            _logger.LogWarning(ex, "Unable to retrieve authentication token for request to {RequestUri}. Request will proceed without authentication.", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
