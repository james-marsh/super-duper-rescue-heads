using Microsoft.AspNetCore.Mvc;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Search;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/search")
            .WithTags("Search")
            .RequireAuthorization();

        // GET /api/v1/search
        group.MapGet("", async (
            [FromQuery] string q,
            ISearchService searchService,
            ICurrentUserService currentUserService,
            CancellationToken ct,
            [FromQuery] Guid? collectionId = null,
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = currentUserService.GetUserId();

            var query = new SearchQuery
            {
                SearchTerm = q,
                CollectionId = collectionId,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Skip = (page - 1) * pageSize,
                Take = pageSize
            };

            try
            {
                var results = await searchService.SearchAsync(query, ct);

                // Save recent search
                await searchService.SaveRecentSearchAsync(userId, q, ct);

                return Results.Ok(results);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Invalid search parameters",
                    status = 400,
                    detail = ex.Message,
                    instance = "/api/v1/search"
                });
            }
        })
        .WithName("SearchItems")
        .WithOpenApi()
        .Produces<SearchResultPage>()
        .ProducesProblem(400)
        .ProducesProblem(401);

        // GET /api/v1/search/suggestions
        group.MapGet("/suggestions", async (
            [FromQuery] string prefix,
            ISearchService searchService,
            ICurrentUserService currentUserService,
            CancellationToken ct,
            [FromQuery] int limit = 5) =>
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 3)
            {
                return Results.BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Invalid prefix",
                    status = 400,
                    detail = "Prefix must be at least 3 characters",
                    instance = "/api/v1/search/suggestions"
                });
            }

            var userId = currentUserService.GetUserId();
            var suggestions = await searchService.GetSuggestionsAsync(prefix, userId, ct);

            return Results.Ok(new { suggestions });
        })
        .WithName("GetSearchSuggestions")
        .WithOpenApi()
        .Produces<object>()
        .ProducesProblem(400)
        .ProducesProblem(401);

        // GET /api/v1/search/recent
        group.MapGet("/recent", async (
            ISearchService searchService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            var userId = currentUserService.GetUserId();
            var recentSearches = await searchService.GetRecentSearchesAsync(userId, ct);

            return Results.Ok(new { recentSearches });
        })
        .WithName("GetRecentSearches")
        .WithOpenApi()
        .Produces<object>()
        .ProducesProblem(401);

        return app;
    }
}
