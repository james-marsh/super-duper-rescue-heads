using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace SuperDuperRescueHeads.Web.Services;

public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthenticationStateProvider authenticationStateProvider)
        : base(options, logger, encoder)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var ticket = new AuthenticationTicket(user, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.NoResult();
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex);
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // For Blazor, we don't redirect here - the RedirectToLogin component handles it
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}
