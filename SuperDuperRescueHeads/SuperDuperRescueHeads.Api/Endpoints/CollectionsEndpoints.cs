using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Collections;
using SuperDuperRescueHeads.Domain.Shared;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class CollectionsEndpoints
{
    public static IEndpointRouteBuilder MapCollectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/collections")
            .WithTags("Collections")
            .RequireAuthorization();

        // GET /api/v1/collections - List all collections for current user
        group.MapGet("", async (
            ICollectionRepository repository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var collections = await repository.GetByOwnerIdAsync(userId, cancellationToken);

            var response = collections.Select(c => new CollectionResponse
            {
                CollectionId = c.CollectionId,
                OwnerId = c.OwnerId,
                Name = c.Name.Value,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ItemCount = c.Items.Count
            });

            return Results.Ok(response);
        })
        .WithName("ListCollections")
        .WithOpenApi();

        // POST /api/v1/collections - Create a new collection
        group.MapPost("", async (
            CreateCollectionRequest request,
            ICollectionRepository repository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var collectionName = CollectionName.Create(request.Name);
            var collection = Collection.Create(userId, collectionName, request.Description);

            await repository.AddAsync(collection, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var response = new CollectionResponse
            {
                CollectionId = collection.CollectionId,
                OwnerId = collection.OwnerId,
                Name = collection.Name.Value,
                Description = collection.Description,
                CreatedAt = collection.CreatedAt,
                UpdatedAt = collection.UpdatedAt,
                ItemCount = 0
            };

            return Results.Created($"/api/v1/collections/{collection.CollectionId}", response);
        })
        .WithName("CreateCollection")
        .WithOpenApi();

        // GET /api/v1/collections/{collectionId} - Get a specific collection
        group.MapGet("{collectionId:guid}", async (
            Guid collectionId,
            ICollectionRepository repository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var collection = await repository.GetByIdAsync(collectionId, cancellationToken);

            if (collection == null)
                throw new NotFoundException("Collection", collectionId);

            // Authorization: Only owner can access
            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("collection", "access");

            var response = new CollectionResponse
            {
                CollectionId = collection.CollectionId,
                OwnerId = collection.OwnerId,
                Name = collection.Name.Value,
                Description = collection.Description,
                CreatedAt = collection.CreatedAt,
                UpdatedAt = collection.UpdatedAt,
                ItemCount = collection.Items.Count
            };

            return Results.Ok(response);
        })
        .WithName("GetCollection")
        .WithOpenApi();

        // PUT /api/v1/collections/{collectionId} - Update a collection
        group.MapPut("{collectionId:guid}", async (
            Guid collectionId,
            UpdateCollectionRequest request,
            ICollectionRepository repository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var collection = await repository.GetByIdAsync(collectionId, cancellationToken);

            if (collection == null)
                throw new NotFoundException("Collection", collectionId);

            // Authorization: Only owner can update
            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("collection", "update");

            try
            {
                var collectionName = CollectionName.Create(request.Name);
                collection.UpdateName(collectionName);
                collection.UpdateDescription(request.Description);

                await repository.UpdateAsync(collection, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                var response = new CollectionResponse
                {
                    CollectionId = collection.CollectionId,
                    OwnerId = collection.OwnerId,
                    Name = collection.Name.Value,
                    Description = collection.Description,
                    CreatedAt = collection.CreatedAt,
                    UpdatedAt = collection.UpdatedAt,
                    ItemCount = collection.Items.Count
                };

                return Results.Ok(response);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException(collectionId, "Collection");
            }
        })
        .WithName("UpdateCollection")
        .WithOpenApi();

        // DELETE /api/v1/collections/{collectionId} - Soft delete a collection
        group.MapDelete("{collectionId:guid}", async (
            Guid collectionId,
            ICollectionRepository repository,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetUserId();

            var collection = await repository.GetByIdAsync(collectionId, cancellationToken);

            if (collection == null)
                throw new NotFoundException("Collection", collectionId);

            // Authorization: Only owner can delete
            if (!collection.IsOwnedBy(userId))
                throw new UnauthorizedException("collection", "delete");

            try
            {
                collection.MarkAsDeleted();
                await repository.UpdateAsync(collection, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                return Results.NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException(collectionId, "Collection");
            }
        })
        .WithName("DeleteCollection")
        .WithOpenApi();

        return app;
    }
}
