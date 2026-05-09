using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Collections;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Shared;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class ItemsEndpoints
{
    public static IEndpointRouteBuilder MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Items")
            .RequireAuthorization();

        // GET /api/v1/collections/{collectionId}/items
        group.MapGet("/collections/{collectionId:guid}/items", async (
            Guid collectionId,
            IItemRepository itemRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            int skip = 0,
            int take = 100,
            CancellationToken cancellationToken = default) =>
        {
            var userId = currentUserService.GetUserId();

            // Verify collection exists and user has access
            var collection = await collectionRepository.GetByIdAsync(collectionId, cancellationToken);
            if (collection == null)
                throw new NotFoundException("Collection", collectionId);

            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("collection", "access");

            // Validate pagination parameters
            if (skip < 0) skip = 0;
            if (take < 1) take = 1;
            if (take > 100) take = 100;

            // Get items and total count
            var items = await itemRepository.GetByCollectionIdAsync(collectionId, skip, take, cancellationToken);
            var total = await itemRepository.CountByCollectionIdAsync(collectionId, cancellationToken);

            var response = items.Select(i => new ItemResponse
            {
                ItemId = i.ItemId,
                CollectionId = i.CollectionId,
                Name = i.Name.Value,
                Notes = i.Notes,
                Attributes = i.Attributes,
                AcquisitionDate = i.AcquisitionDate,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            });

            return Results.Ok(new
            {
                data = response,
                pagination = new
                {
                    total,
                    skip,
                    take,
                    hasMore = skip + take < total
                }
            });
        })
        .WithName("ListItems")
;

        // POST /api/v1/collections/{collectionId}/items
        group.MapPost("/collections/{collectionId:guid}/items", async (
            Guid collectionId,
            CreateItemRequest request,
            IItemRepository itemRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken = default) =>
        {
            var userId = currentUserService.GetUserId();

            // Verify collection exists and user has access
            var collection = await collectionRepository.GetByIdAsync(collectionId, cancellationToken);
            if (collection == null)
                throw new NotFoundException("Collection", collectionId);

            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("collection", "create items in");

            // Create item
            var itemName = ItemName.Create(request.Name);
            var item = Item.Create(
                collectionId,
                itemName,
                request.Attributes,
                request.Notes,
                request.AcquisitionDate);

            await itemRepository.AddAsync(item, cancellationToken);
            await itemRepository.SaveChangesAsync(cancellationToken);

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

            return Results.Created($"/api/v1/items/{item.ItemId}", response);
        })
        .WithName("CreateItem")
;

        // GET /api/v1/items/{itemId}
        group.MapGet("/items/{itemId:guid}", async (
            Guid itemId,
            IItemRepository itemRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken = default) =>
        {
            var userId = currentUserService.GetUserId();

            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
                throw new NotFoundException("Item", itemId);

            // Verify user owns the collection
            var collection = await collectionRepository.GetByIdAsync(item.CollectionId, cancellationToken);
            if (collection == null)
                throw new NotFoundException("Collection", item.CollectionId);

            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("item", "access");

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
        .WithName("GetItem")
;

        // PUT /api/v1/items/{itemId}
        group.MapPut("/items/{itemId:guid}", async (
            Guid itemId,
            UpdateItemRequest request,
            IItemRepository itemRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken = default) =>
        {
            var userId = currentUserService.GetUserId();

            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
                throw new NotFoundException("Item", itemId);

            // Verify user owns the collection
            var collection = await collectionRepository.GetByIdAsync(item.CollectionId, cancellationToken);
            if (collection == null)
                throw new NotFoundException("Collection", item.CollectionId);

            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("item", "update");

            try
            {
                // Update item properties
                var itemName = ItemName.Create(request.Name);
                item.UpdateName(itemName);
                item.UpdateNotes(request.Notes);
                item.UpdateAttributes(request.Attributes);
                item.UpdateAcquisitionDate(request.AcquisitionDate);

                await itemRepository.UpdateAsync(item, cancellationToken);
                await itemRepository.SaveChangesAsync(cancellationToken);

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
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException(itemId, "Item");
            }
        })
        .WithName("UpdateItem")
;

        // DELETE /api/v1/items/{itemId}
        group.MapDelete("/items/{itemId:guid}", async (
            Guid itemId,
            IItemRepository itemRepository,
            ICollectionRepository collectionRepository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken = default) =>
        {
            var userId = currentUserService.GetUserId();

            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
                throw new NotFoundException("Item", itemId);

            // Verify user owns the collection
            var collection = await collectionRepository.GetByIdAsync(item.CollectionId, cancellationToken);
            if (collection == null)
                throw new NotFoundException("Collection", item.CollectionId);

            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("item", "delete");

            try
            {
                item.MarkAsDeleted();
                await itemRepository.UpdateAsync(item, cancellationToken);
                await itemRepository.SaveChangesAsync(cancellationToken);

                return Results.NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException(itemId, "Item");
            }
        })
        .WithName("DeleteItem")
;

        return app;
    }
}
