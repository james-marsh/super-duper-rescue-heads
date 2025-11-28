namespace SuperDuperRescueHeads.Domain.Search;

public interface ISearchService
{
    Task<SearchResultPage> SearchAsync(SearchQuery query, CancellationToken ct = default);
    Task<string[]> GetSuggestionsAsync(string prefix, Guid userId, CancellationToken ct = default);
    Task<string[]> GetRecentSearchesAsync(Guid userId, CancellationToken ct = default);
    Task SaveRecentSearchAsync(Guid userId, string searchTerm, CancellationToken ct = default);
}
