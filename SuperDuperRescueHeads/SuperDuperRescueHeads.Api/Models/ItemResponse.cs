namespace SuperDuperRescueHeads.Api.Models;

public record ItemResponse
{
    public required Guid ItemId { get; init; }
    public required Guid CollectionId { get; init; }
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public required Dictionary<string, object> Attributes { get; init; }
    public DateTimeOffset? AcquisitionDate { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
