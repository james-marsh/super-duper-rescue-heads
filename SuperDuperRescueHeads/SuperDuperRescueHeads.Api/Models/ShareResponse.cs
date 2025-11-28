namespace SuperDuperRescueHeads.Api.Models;

public record ShareResponse
{
    public required Guid CollectionShareId { get; init; }
    public required Guid CollectionId { get; init; }
    public required Guid SharedWithUserId { get; init; }
    public required string Permission { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset InvitedAt { get; init; }
    public DateTimeOffset? AcceptedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? LastAccessedAt { get; init; }
}
