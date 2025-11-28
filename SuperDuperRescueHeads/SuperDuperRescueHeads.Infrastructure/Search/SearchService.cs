using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Search;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Search;

public class SearchService : ISearchService
{
    private readonly ISearchRepository _repository;
    private readonly ApplicationDbContext _context;

    public SearchService(ISearchRepository repository, ApplicationDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<SearchResultPage> SearchAsync(SearchQuery query, CancellationToken ct)
    {
        query.Validate();

        var (items, totalCount) = await _repository.SearchItemsAsync(query, ct);

        var results = items.Select(item => new SearchResult
        {
            ItemId = item.ItemId,
            Name = item.Name.Value,
            Notes = item.Notes,
            CollectionId = item.CollectionId,
            CollectionName = "Collection", // TODO: Load from collection when collection aggregate is implemented
            AcquisitionDate = item.AcquisitionDate,
            CreatedAt = item.CreatedAt,
            RelevanceScore = 0.95m, // TODO: Get from SQL RANK function
            Highlights = ExtractHighlights(query.SearchTerm, item)
        }).ToArray();

        return new SearchResultPage
        {
            Results = results,
            TotalCount = totalCount,
            Page = (query.Skip / query.Take) + 1,
            PageSize = query.Take,
            Query = query
        };
    }

    public async Task<string[]> GetSuggestionsAsync(string prefix, Guid userId, CancellationToken ct)
    {
        if (prefix.Length < 3)
            return Array.Empty<string>();

        return await _repository.GetSuggestionsAsync(prefix, userId, 5, ct);
    }

    public async Task<string[]> GetRecentSearchesAsync(Guid userId, CancellationToken ct)
    {
        return await _context.UserSearchHistory
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.SearchedAt)
            .Take(5)
            .Select(h => h.SearchTerm)
            .ToArrayAsync(ct);
    }

    public async Task SaveRecentSearchAsync(Guid userId, string searchTerm, CancellationToken ct)
    {
        // Update or insert the search term
        var existing = await _context.UserSearchHistory
            .FirstOrDefaultAsync(h => h.UserId == userId && h.SearchTerm == searchTerm, ct);

        if (existing != null)
        {
            existing.SearchedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _context.UserSearchHistory.Add(new Data.Entities.UserSearchHistory
            {
                UserId = userId,
                SearchTerm = searchTerm,
                SearchedAt = DateTimeOffset.UtcNow
            });
        }

        await _context.SaveChangesAsync(ct);

        // Keep only the 5 most recent searches
        var allSearches = await _context.UserSearchHistory
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.SearchedAt)
            .ToListAsync(ct);

        if (allSearches.Count > 5)
        {
            var toRemove = allSearches.Skip(5);
            _context.UserSearchHistory.RemoveRange(toRemove);
            await _context.SaveChangesAsync(ct);
        }
    }

    private string[] ExtractHighlights(string searchTerm, Domain.Items.Item item)
    {
        var keywords = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var highlights = new List<string>();

        foreach (var keyword in keywords)
        {
            if (item.Name.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                highlights.Add(keyword.ToLower());
            }
        }

        return highlights.Distinct().ToArray();
    }
}
