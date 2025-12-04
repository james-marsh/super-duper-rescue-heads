namespace SuperDuperRescueHeads.Web.Services;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string DisplayName, string Password);

public record AuthenticationResponse(
    string Token,
    Guid UserId,
    string Email,
    string DisplayName,
    DateTimeOffset ExpiresAt
);

public record UserInfo(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsAuthenticated
)
{
    public static UserInfo Anonymous => new(Guid.Empty, string.Empty, string.Empty, false);
}
