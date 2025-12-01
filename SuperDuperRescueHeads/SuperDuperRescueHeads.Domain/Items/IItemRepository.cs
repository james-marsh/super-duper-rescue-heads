namespace SuperDuperRescueHeads.Domain.Items;

public interface IItemRepository
{
    // Queries
    Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Item>> GetByCollectionIdAsync(Guid collectionId, int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<int> CountByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Keyset pagination (for large collections)
    Task<IReadOnlyList<Item>> GetByCollectionIdPagedAsync(
        Guid collectionId,
        DateTimeOffset? lastCreatedAt,
        Guid? lastItemId,
        int take = 100,
        CancellationToken cancellationToken = default);

    // Commands
    Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default);
    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);
    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);

    // Concurrency Control (Feature 009)
    Task<Item?> TryReloadAsync(Guid itemId, CancellationToken cancellationToken = default);

    // Soft Delete Methods (Feature 003)
    Task<IReadOnlyList<Item>> GetDeletedItemsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Item?> GetDeletedItemByIdAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task PurgeAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredItemsAsync(int batchSize = 1000, CancellationToken cancellationToken = default);

    // Unit of Work
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
