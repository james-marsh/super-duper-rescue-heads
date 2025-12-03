namespace SuperDuperRescueHeads.Api.Models;

public record CollectionResponse
{
    public required Guid CollectionId { get; init; }
    public required Guid OwnerId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required int ItemCount { get; init; }
}
