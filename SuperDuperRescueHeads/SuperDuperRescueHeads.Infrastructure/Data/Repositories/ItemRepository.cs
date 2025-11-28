using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Infrastructure.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _context;

    public ItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Queries
    public async Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .FirstOrDefaultAsync(i => i.ItemId == itemId, cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> GetByCollectionIdAsync(
        Guid collectionId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Where(i => i.CollectionId == collectionId)
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .CountAsync(i => i.CollectionId == collectionId, cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Placeholder: Requires Collection navigation property from Feature 001
        throw new NotImplementedException("Requires Collection navigation property from Feature 001");
    }

    public async Task<IReadOnlyList<Item>> GetByCollectionIdPagedAsync(
        Guid collectionId,
        DateTimeOffset? lastCreatedAt,
        Guid? lastItemId,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Items
            .Where(i => i.CollectionId == collectionId);

        if (lastCreatedAt.HasValue && lastItemId.HasValue)
        {
            query = query.Where(i =>
                i.CreatedAt < lastCreatedAt.Value ||
                (i.CreatedAt == lastCreatedAt.Value && i.ItemId.CompareTo(lastItemId.Value) < 0));
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ThenByDescending(i => i.ItemId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    // Commands
    public async Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        await _context.Items.AddAsync(item, cancellationToken);
        return item;
    }

    public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Update(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Item item, CancellationToken cancellationToken = default)
    {
        // Soft delete - call MarkAsDeleted on the aggregate
        item.MarkAsDeleted();
        _context.Items.Update(item);
        return Task.CompletedTask;
    }

    // Soft Delete Methods (Feature 003)
    public async Task<IReadOnlyList<Item>> GetDeletedItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Note: This requires Collection navigation property for userId filtering
        // For now, return all deleted items without user filtering
        return await _context.Items
            .IgnoreQueryFilters() // Include deleted items
            .Where(i => i.IsDeleted)
            .OrderByDescending(i => i.DeletedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Item?> GetDeletedItemByIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .IgnoreQueryFilters() // Include deleted items
            .FirstOrDefaultAsync(i => i.ItemId == itemId && i.IsDeleted, cancellationToken);
    }

    public async Task PurgeAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _context.Items
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, cancellationToken);

        if (item != null)
        {
            _context.Items.Remove(item);
        }
    }

    public async Task<int> PurgeExpiredItemsAsync(int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);

        var expiredItems = await _context.Items
            .IgnoreQueryFilters()
            .Where(i => i.IsDeleted && i.DeletedAt < cutoffDate)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        _context.Items.RemoveRange(expiredItems);

        return expiredItems.Count;
    }

    // Unit of Work
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
