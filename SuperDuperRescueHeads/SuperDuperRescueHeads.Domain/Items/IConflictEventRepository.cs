namespace SuperDuperRescueHeads.Domain.Items;

public interface IConflictEventRepository
{
    Task<ConflictEvent?> GetByIdAsync(Guid conflictEventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConflictEvent>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConflictEvent>> GetByUserIdAsync(Guid userId, int take = 100, CancellationToken cancellationToken = default);
    Task<int> GetConflictCountAsync(DateTimeOffset since, CancellationToken cancellationToken = default);

    Task<ConflictEvent> AddAsync(ConflictEvent conflictEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConflictEvent conflictEvent, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
