using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Models;

public record ShareCollectionRequest
{
    public required string Email { get; init; }
    public required Guid SharedWithUserId { get; init; }
    public required SharePermission Permission { get; init; }
}
