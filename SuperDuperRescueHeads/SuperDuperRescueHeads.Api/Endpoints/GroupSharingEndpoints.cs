using Microsoft.AspNetCore.Mvc;
using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Collections;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Domain.Shared;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class GroupSharingEndpoints
{
    public static IEndpointRouteBuilder MapGroupSharingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Group Sharing")
            .RequireAuthorization();

        // POST /api/v1/collections/{collectionId}/share/group
        group.MapPost("/collections/{collectionId:guid}/share/group", async (
            Guid collectionId,
            ShareWithGroupRequest request,
            ICollectionShareRepository shareRepository,
            ICollectionRepository collectionRepository,
            IUserGroupRepository groupRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            // Verify that current user owns the collection
            var collection = await collectionRepository.GetByIdAsync(collectionId, cancellationToken);
            if (collection == null)
            {
                return Results.NotFound(new { error = "Collection not found" });
            }
            if (!collection.IsOwnedBy(currentUserId))
            {
                throw new UnauthorizedException("collection", "share with group");
            }

            // Verify the group exists
            var userGroup = await groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
            if (userGroup == null)
            {
                return Results.NotFound(new { error = "Group not found" });
            }

            // Check if collection is already shared with this group
            var existingShares = await shareRepository.GetGroupSharesByCollectionIdAsync(collectionId, cancellationToken);
            if (existingShares.Any(s => s.GroupId == request.GroupId))
            {
                return Results.BadRequest(new { error = "Collection is already shared with this group" });
            }

            // Create group share
            var share = CollectionShare.CreateGroupInvitation(
                collectionId,
                currentUserId,
                request.GroupId,
                request.Permission);

            await shareRepository.AddAsync(share, cancellationToken);
            await shareRepository.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/shares/{share.CollectionShareId}", new GroupShareResponse
            {
                CollectionShareId = share.CollectionShareId,
                CollectionId = share.CollectionId,
                GroupId = request.GroupId,
                GroupName = userGroup.GroupName,
                Permission = share.Permission.ToString(),
                MemberCount = userGroup.GetMemberCount(),
                CreatedAt = share.CreatedAt
            });
        })
        .WithName("ShareCollectionWithGroup")
        .WithOpenApi();

        // GET /api/v1/groups
        group.MapGet("/groups", async (
            IUserGroupRepository groupRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20) =>
        {
            var currentUserId = currentUserService.GetUserId();

            // Get groups the current user is a member of
            var groups = await groupRepository.GetByUserIdAsync(currentUserId, cancellationToken);

            var responses = groups.Select(g => new GroupResponse
            {
                UserGroupId = g.UserGroupId,
                GroupName = g.GroupName,
                Description = g.Description,
                MemberCount = g.GetMemberCount(),
                CreatedAt = g.CreatedAt
            });

            return Results.Ok(new { data = responses });
        })
        .WithName("ListUserGroups")
        .WithOpenApi();

        // GET /api/v1/collections/{collectionId}/shares/groups
        group.MapGet("/collections/{collectionId:guid}/shares/groups", async (
            Guid collectionId,
            ICollectionShareRepository shareRepository,
            ICollectionRepository collectionRepository,
            IUserGroupRepository groupRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            // Verify that current user owns or has access to the collection
            var collection = await collectionRepository.GetByIdAsync(collectionId, cancellationToken);
            if (collection == null)
            {
                return Results.NotFound(new { error = "Collection not found" });
            }
            if (!collection.IsOwnedBy(currentUserId))
            {
                throw new UnauthorizedException("collection", "view shares for");
            }

            var groupShares = await shareRepository.GetGroupSharesByCollectionIdAsync(collectionId, cancellationToken);

            var responses = new List<GroupShareResponse>();
            foreach (var share in groupShares)
            {
                if (share.GroupId.HasValue)
                {
                    var userGroup = await groupRepository.GetByIdAsync(share.GroupId.Value, cancellationToken);
                    if (userGroup != null)
                    {
                        responses.Add(new GroupShareResponse
                        {
                            CollectionShareId = share.CollectionShareId,
                            CollectionId = share.CollectionId,
                            GroupId = share.GroupId.Value,
                            GroupName = userGroup.GroupName,
                            Permission = share.Permission.ToString(),
                            MemberCount = userGroup.GetMemberCount(),
                            CreatedAt = share.CreatedAt
                        });
                    }
                }
            }

            return Results.Ok(new { data = responses });
        })
        .WithName("ListGroupShares")
        .WithOpenApi();

        // PATCH /api/v1/collections/{collectionId}/shares/group/{groupId}/permission - US3
        group.MapPatch("/collections/{collectionId:guid}/shares/group/{groupId:guid}/permission", async (
            Guid collectionId,
            Guid groupId,
            [FromBody] ShareWithGroupRequest request,
            ICollectionShareRepository shareRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            // Verify current user owns the collection
            var collection = await collectionRepository.GetByIdAsync(collectionId, cancellationToken);
            if (collection == null)
            {
                return Results.NotFound(new { error = "Collection not found" });
            }
            if (!collection.IsOwnedBy(currentUserId))
            {
                throw new UnauthorizedException("collection", "change group permissions for");
            }

            var groupShares = await shareRepository.GetGroupSharesByCollectionIdAsync(collectionId, cancellationToken);
            var share = groupShares.FirstOrDefault(s => s.GroupId == groupId);

            if (share == null)
            {
                return Results.NotFound(new { error = "Group share not found" });
            }

            share.ChangePermission(request.Permission);
            await shareRepository.UpdateAsync(share, cancellationToken);
            await shareRepository.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "Permission updated successfully" });
        })
        .WithName("ChangeGroupPermission")
        .WithOpenApi();

        // DELETE /api/v1/collections/{collectionId}/shares/group/{groupId} - US3
        group.MapDelete("/collections/{collectionId:guid}/shares/group/{groupId:guid}", async (
            Guid collectionId,
            Guid groupId,
            ICollectionShareRepository shareRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            // Verify current user owns the collection
            var collection = await collectionRepository.GetByIdAsync(collectionId, cancellationToken);
            if (collection == null)
            {
                return Results.NotFound(new { error = "Collection not found" });
            }
            if (!collection.IsOwnedBy(currentUserId))
            {
                throw new UnauthorizedException("collection", "revoke group access from");
            }

            var groupShares = await shareRepository.GetGroupSharesByCollectionIdAsync(collectionId, cancellationToken);
            var share = groupShares.FirstOrDefault(s => s.GroupId == groupId);

            if (share == null)
            {
                return Results.NotFound(new { error = "Group share not found" });
            }

            await shareRepository.DeleteAsync(share, cancellationToken);
            await shareRepository.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("RevokeGroupAccess")
        .WithOpenApi();

        // GET /api/v1/collections/{collectionId}/access-sources - US3
        group.MapGet("/collections/{collectionId:guid}/access-sources", async (
            Guid collectionId,
            ICollectionShareRepository shareRepository,
            IUserGroupRepository groupRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            var allShares = await shareRepository.GetByCollectionIdAsync(collectionId, cancellationToken);

            var sources = new List<AccessSourceResponse>();

            // Check for individual access
            var individualShare = allShares.FirstOrDefault(s =>
                s.SharedWithUserId == currentUserId &&
                s.Status == ShareStatus.Accepted &&
                !s.IsGroupShare);

            if (individualShare != null)
            {
                sources.Add(new AccessSourceResponse
                {
                    Type = "Individual",
                    Permission = individualShare.Permission.ToString(),
                    GrantedAt = individualShare.AcceptedAt ?? individualShare.InvitedAt
                });
            }

            // Check for group access
            var userGroups = await groupRepository.GetByUserIdAsync(currentUserId, cancellationToken);
            var userGroupIds = userGroups.Select(g => g.UserGroupId).ToHashSet();

            var groupShares = allShares.Where(s =>
                s.GroupId.HasValue &&
                s.IsGroupShare &&
                userGroupIds.Contains(s.GroupId.Value)).ToList();

            foreach (var groupShare in groupShares)
            {
                var userGroup = userGroups.FirstOrDefault(g => g.UserGroupId == groupShare.GroupId!.Value);
                if (userGroup != null)
                {
                    sources.Add(new AccessSourceResponse
                    {
                        Type = "Group",
                        GroupId = groupShare.GroupId,
                        GroupName = userGroup.GroupName,
                        Permission = groupShare.Permission.ToString(),
                        GrantedAt = groupShare.CreatedAt
                    });
                }
            }

            // Determine effective permission (most permissive wins)
            var effectivePermission = sources.Any()
                ? sources.Max(s => Enum.Parse<SharePermission>(s.Permission))
                : (SharePermission?)null;

            return Results.Ok(new
            {
                collectionId,
                userId = currentUserId,
                hasAccess = sources.Any(),
                effectivePermission = effectivePermission?.ToString(),
                sources
            });
        })
        .WithName("GetAccessSources")
        .WithOpenApi();

        return app;
    }
}
