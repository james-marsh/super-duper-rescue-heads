using Microsoft.AspNetCore.Authorization;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Authorization;

public class CollectionPermissionRequirement : IAuthorizationRequirement
{
    public SharePermission MinimumPermission { get; }

    public CollectionPermissionRequirement(SharePermission minimumPermission)
    {
        MinimumPermission = minimumPermission;
    }
}
