namespace SuperDuperRescueHeads.Api.Models;

public record CreateItemRequest
{
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public required Dictionary<string, object> Attributes { get; init; }
    public DateTimeOffset? AcquisitionDate { get; init; }
}
