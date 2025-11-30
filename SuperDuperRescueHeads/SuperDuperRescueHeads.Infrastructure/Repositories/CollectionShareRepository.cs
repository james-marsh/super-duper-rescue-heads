using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Sharing;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Repositories;

public class CollectionShareRepository : ICollectionShareRepository
{
    private readonly ApplicationDbContext _context;

    public CollectionShareRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CollectionShare?> GetByIdAsync(Guid collectionShareId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.CollectionShareId == collectionShareId, cancellationToken);
    }

    public async Task<CollectionShare?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.InvitationToken == token, cancellationToken);
    }

    public async Task<List<CollectionShare>> GetByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .Where(cs => cs.CollectionId == collectionId)
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CollectionShare>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .Where(cs => cs.SharedWithUserId == userId)
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CollectionShare>> GetPendingInvitationsByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .Where(cs => cs.CollectionId == collectionId && cs.Status == ShareStatus.Pending)
            .OrderByDescending(cs => cs.InvitedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveCollaboratorCountAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .CountAsync(cs => cs.CollectionId == collectionId && cs.Status == ShareStatus.Accepted, cancellationToken);
    }

    public async Task<bool> HasActiveShareAsync(Guid collectionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .AnyAsync(cs => cs.CollectionId == collectionId
                && cs.SharedWithUserId == userId
                && cs.Status == ShareStatus.Accepted, cancellationToken);
    }

    // Feature 007: Group sharing methods
    public async Task<List<CollectionShare>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .Where(cs => cs.GroupId == groupId)
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CollectionShare>> GetGroupSharesByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionShares
            .AsNoTracking()
            .Where(cs => cs.CollectionId == collectionId && cs.GroupId != null)
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CollectionShare share, CancellationToken cancellationToken = default)
    {
        await _context.CollectionShares.AddAsync(share, cancellationToken);
    }

    public Task UpdateAsync(CollectionShare share, CancellationToken cancellationToken = default)
    {
        _context.CollectionShares.Update(share);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CollectionShare share, CancellationToken cancellationToken = default)
    {
        _context.CollectionShares.Remove(share);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
