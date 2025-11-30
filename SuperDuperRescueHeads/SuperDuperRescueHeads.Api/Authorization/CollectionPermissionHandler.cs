using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Authorization;

public class CollectionPermissionHandler : AuthorizationHandler<CollectionPermissionRequirement>
{
    private readonly ICollectionShareRepository _shareRepository;
    private readonly IUserGroupRepository _groupRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;

    public CollectionPermissionHandler(
        ICollectionShareRepository shareRepository,
        IUserGroupRepository groupRepository,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
    {
        _shareRepository = shareRepository;
        _groupRepository = groupRepository;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
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

        // Check effective permission (individual + group shares)
        var effectivePermission = await GetEffectivePermissionAsync(collectionId, userId);

        if (effectivePermission.HasValue && effectivePermission.Value >= requirement.MinimumPermission)
        {
            context.Succeed(requirement);
        }
    }

    // Feature 007: Get effective permission from all sources (individual + groups)
    private async Task<SharePermission?> GetEffectivePermissionAsync(Guid collectionId, Guid userId)
    {
        var shares = await _shareRepository.GetByCollectionIdAsync(collectionId);

        // Check individual share
        var individualShare = shares.FirstOrDefault(s =>
            s.SharedWithUserId == userId &&
            s.Status == ShareStatus.Accepted &&
            !s.IsGroupShare);

        SharePermission? maxPermission = individualShare?.Permission;

        // Check group shares (with 5-minute cache)
        var userGroups = await GetUserGroupsAsync(userId);

        foreach (var userGroup in userGroups)
        {
            var groupShare = shares.FirstOrDefault(s =>
                s.GroupId == userGroup.UserGroupId &&
                s.IsGroupShare);

            if (groupShare != null)
            {
                // Most permissive wins
                if (!maxPermission.HasValue || groupShare.Permission > maxPermission.Value)
                {
                    maxPermission = groupShare.Permission;
                }
            }
        }

        // Update last accessed if we have access
        if (maxPermission.HasValue && individualShare != null)
        {
            individualShare.UpdateLastAccessed();
            await _shareRepository.UpdateAsync(individualShare);
            await _shareRepository.SaveChangesAsync();
        }

        return maxPermission;
    }

    private async Task<List<UserGroup>> GetUserGroupsAsync(Guid userId)
    {
        var cacheKey = $"user_groups_{userId}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _groupRepository.GetByUserIdAsync(userId);
        }) ?? new List<UserGroup>();
    }
}
