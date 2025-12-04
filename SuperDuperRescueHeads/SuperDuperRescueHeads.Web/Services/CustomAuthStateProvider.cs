using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace SuperDuperRescueHeads.Web.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthenticationService _authService;

    public CustomAuthStateProvider(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var userInfo = await _authService.GetCurrentUserInfoAsync();

        if (!userInfo.IsAuthenticated)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()),
            new Claim(ClaimTypes.Email, userInfo.Email),
            new Claim(ClaimTypes.Name, userInfo.DisplayName),
            new Claim("domain_user_id", userInfo.UserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(UserInfo userInfo)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()),
            new Claim(ClaimTypes.Email, userInfo.Email),
            new Claim(ClaimTypes.Name, userInfo.DisplayName),
            new Claim("domain_user_id", userInfo.UserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
