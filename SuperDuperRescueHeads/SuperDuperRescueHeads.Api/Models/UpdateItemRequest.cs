namespace SuperDuperRescueHeads.Api.Models;

public record UpdateItemRequest
{
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public Dictionary<string, object>? Attributes { get; init; }
    public DateTimeOffset? AcquisitionDate { get; init; }
}
