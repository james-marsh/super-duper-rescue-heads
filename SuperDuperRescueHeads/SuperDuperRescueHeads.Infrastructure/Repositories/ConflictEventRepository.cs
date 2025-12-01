using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Repositories;

public class ConflictEventRepository : IConflictEventRepository
{
    private readonly ApplicationDbContext _context;

    public ConflictEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConflictEvent?> GetByIdAsync(Guid conflictEventId, CancellationToken cancellationToken = default)
    {
        return await _context.ConflictEvents
            .FirstOrDefaultAsync(c => c.ConflictEventId == conflictEventId, cancellationToken);
    }

    public async Task<IReadOnlyList<ConflictEvent>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _context.ConflictEvents
            .Where(c => c.ItemId == itemId)
            .OrderByDescending(c => c.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConflictEvent>> GetByUserIdAsync(Guid userId, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _context.ConflictEvents
            .Where(c => c.LosingUserId == userId || c.WinningUserId == userId)
            .OrderByDescending(c => c.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetConflictCountAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        return await _context.ConflictEvents
            .CountAsync(c => c.OccurredAt >= since, cancellationToken);
    }

    public async Task<ConflictEvent> AddAsync(ConflictEvent conflictEvent, CancellationToken cancellationToken = default)
    {
        await _context.ConflictEvents.AddAsync(conflictEvent, cancellationToken);
        return conflictEvent;
    }

    public Task UpdateAsync(ConflictEvent conflictEvent, CancellationToken cancellationToken = default)
    {
        _context.ConflictEvents.Update(conflictEvent);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
