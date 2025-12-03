namespace SuperDuperRescueHeads.Api.Models;

public record CreateCollectionRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
