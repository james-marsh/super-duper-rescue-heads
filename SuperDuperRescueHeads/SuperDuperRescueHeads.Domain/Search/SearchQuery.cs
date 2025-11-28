namespace SuperDuperRescueHeads.Domain.Search;

public record SearchQuery
{
    public required string SearchTerm { get; init; }
    public Guid? CollectionId { get; init; }
    public required Guid UserId { get; init; }
    public SearchScope Scope => CollectionId.HasValue ? SearchScope.CollectionSpecific : SearchScope.Global;
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 20;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
            throw new ArgumentException("Search term cannot be empty", nameof(SearchTerm));

        if (SearchTerm.Length > 200)
            throw new ArgumentException("Search term cannot exceed 200 characters", nameof(SearchTerm));

        if (Skip < 0)
            throw new ArgumentException("Skip must be >= 0", nameof(Skip));

        if (Take < 1 || Take > 100)
            throw new ArgumentException("Take must be between 1 and 100", nameof(Take));

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            throw new ArgumentException("StartDate must be <= EndDate");
    }
}

public enum SearchScope
{
    Global = 0,
    CollectionSpecific = 1
}
