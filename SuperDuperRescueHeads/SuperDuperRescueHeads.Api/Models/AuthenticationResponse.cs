namespace SuperDuperRescueHeads.Api.Models;

public record AuthenticationResponse
{
    public required string Token { get; init; }
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
