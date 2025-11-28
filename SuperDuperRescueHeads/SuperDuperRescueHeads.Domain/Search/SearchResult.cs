namespace SuperDuperRescueHeads.Domain.Search;

public record SearchResult
{
    public required Guid ItemId { get; init; }
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public required Guid CollectionId { get; init; }
    public required string CollectionName { get; init; }
    public DateTimeOffset? AcquisitionDate { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required decimal RelevanceScore { get; init; }
    public required string[] Highlights { get; init; }
}

public record SearchResultPage
{
    public required SearchResult[] Results { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public bool HasNextPage => (Page * PageSize) < TotalCount;
    public bool HasPreviousPage => Page > 1;
    public SearchQuery? Query { get; init; }
}
