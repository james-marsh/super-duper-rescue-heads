using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Domain.Search;

public interface ISearchRepository
{
    Task<(List<Item> items, int totalCount)> SearchItemsAsync(SearchQuery query, CancellationToken ct = default);
    Task<string[]> GetSuggestionsAsync(string prefix, Guid userId, int limit, CancellationToken ct = default);
}
