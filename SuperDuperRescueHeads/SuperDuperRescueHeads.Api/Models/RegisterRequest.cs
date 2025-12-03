namespace SuperDuperRescueHeads.Api.Models;

public record RegisterRequest
{
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string Password { get; init; }
}
