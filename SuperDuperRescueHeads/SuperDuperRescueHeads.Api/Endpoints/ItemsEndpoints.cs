using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class ItemsEndpoints
{
    public static IEndpointRouteBuilder MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Items")
            .RequireAuthorization(); // TODO: Add authentication from Feature 001

        // GET /api/v1/collections/{collectionId}/items
        group.MapGet("/collections/{collectionId:guid}/items", async (
            Guid collectionId,
            int skip,
            int take,
            IItemRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Implement list items endpoint
            return Results.Ok(new { data = Array.Empty<ItemResponse>(), pagination = new { total = 0, skip, take, hasMore = false } });
        })
        .WithName("ListItems")
        .WithOpenApi();

        // POST /api/v1/collections/{collectionId}/items
        group.MapPost("/collections/{collectionId:guid}/items", async (
            Guid collectionId,
            CreateItemRequest request,
            IItemRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Implement create item endpoint
            return Results.Problem("Not implemented", statusCode: 501);
        })
        .WithName("CreateItem")
        .WithOpenApi();

        // GET /api/v1/items/{itemId}
        group.MapGet("/items/{itemId:guid}", async (
            Guid itemId,
            IItemRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Implement get item endpoint
            return Results.Problem("Not implemented", statusCode: 501);
        })
        .WithName("GetItem")
        .WithOpenApi();

        // PUT /api/v1/items/{itemId}
        group.MapPut("/items/{itemId:guid}", async (
            Guid itemId,
            UpdateItemRequest request,
            IItemRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Implement update item endpoint
            return Results.Problem("Not implemented", statusCode: 501);
        })
        .WithName("UpdateItem")
        .WithOpenApi();

        // DELETE /api/v1/items/{itemId}
        group.MapDelete("/items/{itemId:guid}", async (
            Guid itemId,
            IItemRepository repository,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Implement delete item endpoint
            return Results.Problem("Not implemented", statusCode: 501);
        })
        .WithName("DeleteItem")
        .WithOpenApi();

        return app;
    }
}
