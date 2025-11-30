namespace SuperDuperRescueHeads.Api.Models;

public record GroupShareResponse
{
    public required Guid CollectionShareId { get; init; }
    public required Guid CollectionId { get; init; }
    public required Guid GroupId { get; init; }
    public required string GroupName { get; init; }
    public required string Permission { get; init; }
    public required int MemberCount { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
