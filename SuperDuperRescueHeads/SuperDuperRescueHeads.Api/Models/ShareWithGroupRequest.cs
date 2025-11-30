using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Models;

public record ShareWithGroupRequest
{
    public required Guid GroupId { get; init; }
    public required SharePermission Permission { get; init; }
}
