using Microsoft.AspNetCore.Mvc;
using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class CollectionSharingEndpoints
{
    public static IEndpointRouteBuilder MapCollectionSharingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Collection Sharing")
            .RequireAuthorization();

        // POST /api/v1/collections/{collectionId}/share
        group.MapPost("/collections/{collectionId:guid}/share", async (
            Guid collectionId,
            ShareCollectionRequest request,
            ICollectionShareRepository repository,
            IEmailService emailService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            // TODO: Validate that current user owns the collection

            // Check if user already has an active share
            var existingShare = await repository.HasActiveShareAsync(collectionId, request.SharedWithUserId, cancellationToken);
            if (existingShare)
            {
                return Results.BadRequest(new { error = "User already has access to this collection" });
            }

            // Get active collaborator count
            var collaboratorCount = await repository.GetActiveCollaboratorCountAsync(collectionId, cancellationToken);
            if (collaboratorCount >= 10) // Max 10 collaborators
            {
                return Results.BadRequest(new { error = "Maximum collaborator limit reached (10)" });
            }

            // Create invitation
            var share = CollectionShare.CreateInvitation(
                collectionId,
                currentUserId,
                request.Email,
                request.SharedWithUserId,
                request.Permission);

            await repository.AddAsync(share, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            // Send invitation email
            await emailService.SendInvitationEmailAsync(
                request.Email,
                "Current User", // TODO: Get from user context
                "Collection Name", // TODO: Get from collection
                share.InvitationToken,
                cancellationToken);

            return Results.Created($"/api/v1/shares/{share.CollectionShareId}", new ShareResponse
            {
                CollectionShareId = share.CollectionShareId,
                CollectionId = share.CollectionId,
                SharedWithUserId = share.SharedWithUserId,
                Permission = share.Permission.ToString(),
                Status = share.Status.ToString(),
                InvitedAt = share.InvitedAt,
                ExpiresAt = share.ExpiresAt
            });
        })
        .WithName("ShareCollection")
        .WithOpenApi();

        // GET /api/v1/invitations/pending
        group.MapGet("/invitations/pending", async (
            ICollectionShareRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            var shares = await repository.GetByUserIdAsync(currentUserId, cancellationToken);
            var pendingShares = shares.Where(s => s.Status == ShareStatus.Pending && s.ExpiresAt > DateTimeOffset.UtcNow).ToList();

            var responses = pendingShares.Select(s => new ShareResponse
            {
                CollectionShareId = s.CollectionShareId,
                CollectionId = s.CollectionId,
                SharedWithUserId = s.SharedWithUserId,
                Permission = s.Permission.ToString(),
                Status = s.Status.ToString(),
                InvitedAt = s.InvitedAt,
                ExpiresAt = s.ExpiresAt
            });

            return Results.Ok(new { data = responses });
        })
        .WithName("GetPendingInvitations")
        .WithOpenApi();

        // POST /api/v1/invitations/{token}/accept
        group.MapPost("/invitations/{token}/accept", async (
            string token,
            ICollectionShareRepository repository,
            IEmailService emailService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var share = await repository.GetByTokenAsync(token, cancellationToken);
            if (share == null)
            {
                return Results.NotFound(new { error = "Invitation not found" });
            }

            try
            {
                share.Accept();
                await repository.UpdateAsync(share, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                // Send notification email to collection owner
                await emailService.SendInvitationAcceptedEmailAsync(
                    "owner@example.com", // TODO: Get owner email
                    "Accepter Name", // TODO: Get from user context
                    "Collection Name", // TODO: Get from collection
                    cancellationToken);

                return Results.Ok(new ShareResponse
                {
                    CollectionShareId = share.CollectionShareId,
                    CollectionId = share.CollectionId,
                    SharedWithUserId = share.SharedWithUserId,
                    Permission = share.Permission.ToString(),
                    Status = share.Status.ToString(),
                    InvitedAt = share.InvitedAt,
                    AcceptedAt = share.AcceptedAt,
                    ExpiresAt = share.ExpiresAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("AcceptInvitation")
        .WithOpenApi();

        // POST /api/v1/invitations/{token}/decline
        group.MapPost("/invitations/{token}/decline", async (
            string token,
            ICollectionShareRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var share = await repository.GetByTokenAsync(token, cancellationToken);
            if (share == null)
            {
                return Results.NotFound(new { error = "Invitation not found" });
            }

            try
            {
                share.Decline();
                await repository.UpdateAsync(share, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                return Results.Ok(new ShareResponse
                {
                    CollectionShareId = share.CollectionShareId,
                    CollectionId = share.CollectionId,
                    SharedWithUserId = share.SharedWithUserId,
                    Permission = share.Permission.ToString(),
                    Status = share.Status.ToString(),
                    InvitedAt = share.InvitedAt,
                    ExpiresAt = share.ExpiresAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("DeclineInvitation")
        .WithOpenApi();

        // DELETE /api/v1/collections/{collectionId}/collaborators/{userId}
        group.MapDelete("/collections/{collectionId:guid}/collaborators/{userId:guid}", async (
            Guid collectionId,
            Guid userId,
            ICollectionShareRepository repository,
            IEmailService emailService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            // TODO: Verify that current user owns the collection

            var shares = await repository.GetByCollectionIdAsync(collectionId, cancellationToken);
            var share = shares.FirstOrDefault(s => s.SharedWithUserId == userId && s.Status == ShareStatus.Accepted);

            if (share == null)
            {
                return Results.NotFound(new { error = "Collaborator not found" });
            }

            share.Revoke();
            await repository.UpdateAsync(share, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            // Send notification email
            await emailService.SendInvitationRevokedEmailAsync(
                share.Email,
                "Collection Name", // TODO: Get from collection
                cancellationToken);

            return Results.NoContent();
        })
        .WithName("RemoveCollaborator")
        .WithOpenApi();

        // PATCH /api/v1/collections/{collectionId}/collaborators/{userId}/permission
        group.MapPatch("/collections/{collectionId:guid}/collaborators/{userId:guid}/permission", async (
            Guid collectionId,
            Guid userId,
            [FromBody] ChangePermissionRequest request,
            ICollectionShareRepository repository,
            IEmailService emailService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            // TODO: Verify that current user owns the collection

            var shares = await repository.GetByCollectionIdAsync(collectionId, cancellationToken);
            var share = shares.FirstOrDefault(s => s.SharedWithUserId == userId && s.Status == ShareStatus.Accepted);

            if (share == null)
            {
                return Results.NotFound(new { error = "Collaborator not found" });
            }

            try
            {
                share.ChangePermission(request.Permission);
                await repository.UpdateAsync(share, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                // Send notification email
                await emailService.SendPermissionChangedEmailAsync(
                    share.Email,
                    "Collection Name", // TODO: Get from collection
                    request.Permission.ToString(),
                    cancellationToken);

                return Results.Ok(new ShareResponse
                {
                    CollectionShareId = share.CollectionShareId,
                    CollectionId = share.CollectionId,
                    SharedWithUserId = share.SharedWithUserId,
                    Permission = share.Permission.ToString(),
                    Status = share.Status.ToString(),
                    InvitedAt = share.InvitedAt,
                    AcceptedAt = share.AcceptedAt,
                    ExpiresAt = share.ExpiresAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ChangeCollaboratorPermission")
        .WithOpenApi();

        // GET /api/v1/collections/{collectionId}/collaborators
        group.MapGet("/collections/{collectionId:guid}/collaborators", async (
            Guid collectionId,
            ICollectionShareRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var shares = await repository.GetByCollectionIdAsync(collectionId, cancellationToken);
            var activeShares = shares.Where(s => s.Status == ShareStatus.Accepted).ToList();

            var responses = activeShares.Select(s => new ShareResponse
            {
                CollectionShareId = s.CollectionShareId,
                CollectionId = s.CollectionId,
                SharedWithUserId = s.SharedWithUserId,
                Permission = s.Permission.ToString(),
                Status = s.Status.ToString(),
                InvitedAt = s.InvitedAt,
                AcceptedAt = s.AcceptedAt,
                ExpiresAt = s.ExpiresAt,
                LastAccessedAt = s.LastAccessedAt
            });

            return Results.Ok(new { data = responses });
        })
        .WithName("ListCollaborators")
        .WithOpenApi();

        return app;
    }
}
