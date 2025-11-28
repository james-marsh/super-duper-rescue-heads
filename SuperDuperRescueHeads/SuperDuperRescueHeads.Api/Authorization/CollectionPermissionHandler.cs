using Microsoft.AspNetCore.Authorization;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Authorization;

public class CollectionPermissionHandler : AuthorizationHandler<CollectionPermissionRequirement>
{
    private readonly ICollectionShareRepository _shareRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CollectionPermissionHandler(
        ICollectionShareRepository shareRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _shareRepository = shareRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CollectionPermissionRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        // TODO: Get user ID from claims
        var userId = Guid.Empty;

        // Get collection ID from route data
        if (!httpContext.Request.RouteValues.TryGetValue("collectionId", out var collectionIdObj) ||
            !Guid.TryParse(collectionIdObj?.ToString(), out var collectionId))
        {
            return;
        }

        // Check if user has active share with required permission
        var shares = await _shareRepository.GetByCollectionIdAsync(collectionId);
        var userShare = shares.FirstOrDefault(s =>
            s.SharedWithUserId == userId &&
            s.Status == ShareStatus.Accepted &&
            s.Permission >= requirement.MinimumPermission);

        if (userShare != null)
        {
            // Update last accessed timestamp
            userShare.UpdateLastAccessed();
            await _shareRepository.UpdateAsync(userShare);
            await _shareRepository.SaveChangesAsync();

            context.Succeed(requirement);
        }
    }
}
