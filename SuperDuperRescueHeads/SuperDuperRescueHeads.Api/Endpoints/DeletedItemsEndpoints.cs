using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Collections;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Shared;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class DeletedItemsEndpoints
{
    public static IEndpointRouteBuilder MapDeletedItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Deleted Items")
            .RequireAuthorization();

        // GET /api/v1/deleted-items
        // User Story 2: View Deleted Items
        group.MapGet("/deleted-items", async (
            IItemRepository repository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var deletedItems = await repository.GetDeletedItemsAsync(userId, cancellationToken);

            var response = deletedItems.Select(item => new DeletedItemResponse(
                item.ItemId,
                item.CollectionId,
                item.Name.Value,
                item.Notes,
                item.Attributes,
                item.AcquisitionDate,
                item.CreatedAt,
                item.DeletedAt!.Value
            ));

            return Results.Ok(response);
        })
        .WithName("GetDeletedItems")
        .WithSummary("Get all deleted items for the authenticated user")
        .WithDescription("Returns a list of items that have been soft-deleted within the last 30 days")
        .WithOpenApi();

        // POST /api/v1/deleted-items/{itemId}/restore
        // User Story 3: Restore Deleted Items
        group.MapPost("/deleted-items/{itemId:guid}/restore", async (
            Guid itemId,
            IItemRepository repository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var item = await repository.GetDeletedItemByIdAsync(itemId, cancellationToken);

            if (item == null)
            {
                return Results.NotFound(new { message = "Deleted item not found" });
            }

            // Check if user owns this item via Collection navigation property
            var collection = await collectionRepository.GetByIdAsync(item.CollectionId, cancellationToken);
            if (collection == null || !collection.IsOwnedBy(userId))
            {
                throw new UnauthorizedException("item", "restore");
            }

            item.Restore();
            await repository.UpdateAsync(item, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var response = new ItemResponse
            {
                ItemId = item.ItemId,
                CollectionId = item.CollectionId,
                Name = item.Name.Value,
                Notes = item.Notes,
                Attributes = item.Attributes,
                AcquisitionDate = item.AcquisitionDate,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };

            return Results.Ok(response);
        })
        .WithName("RestoreDeletedItem")
        .WithSummary("Restore a deleted item")
        .WithDescription("Restores a soft-deleted item, making it visible again in the collection")
        .WithOpenApi();

        // POST /api/v1/deleted-items/{itemId}/purge
        // User Story 4: Permanent Delete
        group.MapPost("/deleted-items/{itemId:guid}/purge", async (
            Guid itemId,
            IItemRepository repository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var item = await repository.GetDeletedItemByIdAsync(itemId, cancellationToken);

            if (item == null)
            {
                return Results.NotFound(new { message = "Deleted item not found" });
            }

            // Check if user owns this item via Collection navigation property
            var collection = await collectionRepository.GetByIdAsync(item.CollectionId, cancellationToken);
            if (collection == null || !collection.IsOwnedBy(userId))
            {
                throw new UnauthorizedException("item", "purge");
            }

            await repository.PurgeAsync(itemId, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("PurgeDeletedItem")
        .WithSummary("Permanently delete an item")
        .WithDescription("Permanently removes a soft-deleted item from the database. This action cannot be undone.")
        .WithOpenApi();

        return app;
    }
}
