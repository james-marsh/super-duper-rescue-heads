namespace SuperDuperRescueHeads.Api.Models;

public record UpdateCollectionRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
