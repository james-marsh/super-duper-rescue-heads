using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SuperDuperRescueHeads.Web.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResponse?> LoginAsync(LoginRequest request);
    Task<AuthenticationResponse?> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<UserInfo> GetCurrentUserInfoAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedSessionStorage _sessionStorage;
    private const string TokenKey = "authToken";
    private const string UserInfoKey = "userInfo";

    public AuthenticationService(HttpClient httpClient, ProtectedSessionStorage sessionStorage)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
    }

    public async Task<AuthenticationResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (authResponse != null)
            {
                await _sessionStorage.SetAsync(TokenKey, authResponse.Token);
                await _sessionStorage.SetAsync(UserInfoKey, new UserInfo(
                    authResponse.UserId,
                    authResponse.Email,
                    authResponse.DisplayName,
                    true
                ));
            }

            return authResponse;
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthenticationResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/register", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (authResponse != null)
            {
                await _sessionStorage.SetAsync(TokenKey, authResponse.Token);
                await _sessionStorage.SetAsync(UserInfoKey, new UserInfo(
                    authResponse.UserId,
                    authResponse.Email,
                    authResponse.DisplayName,
                    true
                ));
            }

            return authResponse;
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await _sessionStorage.DeleteAsync(TokenKey);
        await _sessionStorage.DeleteAsync(UserInfoKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserInfo> GetCurrentUserInfoAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<UserInfo>(UserInfoKey);
            return result.Success ? result.Value ?? UserInfo.Anonymous : UserInfo.Anonymous;
        }
        catch
        {
            return UserInfo.Anonymous;
        }
    }
}
