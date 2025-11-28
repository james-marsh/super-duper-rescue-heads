namespace SuperDuperRescueHeads.Api.Models;

public record DeletedItemResponse(
    Guid ItemId,
    Guid CollectionId,
    string Name,
    string? Notes,
    Dictionary<string, object> Attributes,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset DeletedAt
);
