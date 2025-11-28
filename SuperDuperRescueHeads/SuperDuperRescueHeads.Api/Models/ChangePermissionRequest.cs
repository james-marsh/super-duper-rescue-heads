using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Models;

public record ChangePermissionRequest
{
    public required SharePermission Permission { get; init; }
}
