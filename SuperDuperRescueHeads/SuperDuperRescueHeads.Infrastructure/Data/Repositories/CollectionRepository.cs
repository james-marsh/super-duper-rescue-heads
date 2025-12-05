using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Collection aggregate
/// </summary>
public class CollectionRepository : ICollectionRepository
{
    private readonly ApplicationDbContext _context;

    public CollectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Collection?> GetByIdAsync(Guid collectionId, CancellationToken ct = default)
    {
        return await _context.Collections
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId, ct);
    }

    public async Task<IReadOnlyList<Collection>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.Collections
            .Include(c => c.Items)
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Collection?> GetDeletedCollectionByIdAsync(Guid collectionId, CancellationToken ct = default)
    {
        return await _context.Collections
            .IgnoreQueryFilters()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId && c.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Collection>> GetDeletedCollectionsByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.Collections
            .IgnoreQueryFilters()
            .Include(c => c.Items)
            .Where(c => c.OwnerId == ownerId && c.IsDeleted)
            .OrderByDescending(c => c.DeletedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Collection collection, CancellationToken ct = default)
    {
        await _context.Collections.AddAsync(collection, ct);
    }

    public Task UpdateAsync(Collection collection, CancellationToken ct = default)
    {
        _context.Collections.Update(collection);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Collection collection, CancellationToken ct = default)
    {
        collection.MarkAsDeleted();
        _context.Collections.Update(collection);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid collectionId, CancellationToken ct = default)
    {
        return await _context.Collections
            .AnyAsync(c => c.CollectionId == collectionId, ct);
    }

    public async Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.Collections
            .Where(c => c.OwnerId == ownerId)
            .CountAsync(ct);
    }

    public async Task<int> CountItemsAsync(Guid collectionId, CancellationToken ct = default)
    {
        return await _context.Items
            .Where(i => i.CollectionId == collectionId)
            .CountAsync(ct);
    }

    public async Task<Collection?> TryReloadAsync(Guid collectionId, CancellationToken ct = default)
    {
        var collection = await _context.Collections
            .IgnoreQueryFilters()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId, ct);

        if (collection != null)
        {
            await _context.Entry(collection).ReloadAsync(ct);
        }

        return collection;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
