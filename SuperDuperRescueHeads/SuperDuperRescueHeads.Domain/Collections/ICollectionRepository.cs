namespace SuperDuperRescueHeads.Domain.Collections;

/// <summary>
/// Repository interface for Collection aggregate
/// </summary>
public interface ICollectionRepository
{
    /// <summary>
    /// Gets a collection by ID (excluding soft-deleted)
    /// </summary>
    Task<Collection?> GetByIdAsync(Guid collectionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all collections for a specific owner (excluding soft-deleted)
    /// </summary>
    Task<IReadOnlyList<Collection>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Gets a deleted collection by ID
    /// </summary>
    Task<Collection?> GetDeletedCollectionByIdAsync(Guid collectionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all deleted collections for a specific owner
    /// </summary>
    Task<IReadOnlyList<Collection>> GetDeletedCollectionsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new collection
    /// </summary>
    Task AddAsync(Collection collection, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing collection
    /// </summary>
    Task UpdateAsync(Collection collection, CancellationToken ct = default);

    /// <summary>
    /// Soft deletes a collection
    /// </summary>
    Task DeleteAsync(Collection collection, CancellationToken ct = default);

    /// <summary>
    /// Checks if a collection exists
    /// </summary>
    Task<bool> ExistsAsync(Guid collectionId, CancellationToken ct = default);

    /// <summary>
    /// Counts the number of collections for a specific owner (excluding soft-deleted)
    /// </summary>
    Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Counts items in a collection
    /// </summary>
    Task<int> CountItemsAsync(Guid collectionId, CancellationToken ct = default);

    /// <summary>
    /// Reloads a collection from the database to get the latest version
    /// </summary>
    Task<Collection?> TryReloadAsync(Guid collectionId, CancellationToken ct = default);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
