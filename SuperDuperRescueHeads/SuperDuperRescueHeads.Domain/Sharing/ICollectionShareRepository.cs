namespace SuperDuperRescueHeads.Domain.Sharing;

public interface ICollectionShareRepository
{
    Task<CollectionShare?> GetByIdAsync(Guid collectionShareId, CancellationToken cancellationToken = default);
    Task<CollectionShare?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<CollectionShare>> GetByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<List<CollectionShare>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<CollectionShare>> GetPendingInvitationsByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<int> GetActiveCollaboratorCountAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveShareAsync(Guid collectionId, Guid userId, CancellationToken cancellationToken = default);

    // Feature 007: Group sharing methods
    Task<List<CollectionShare>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<List<CollectionShare>> GetGroupSharesByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default);

    Task AddAsync(CollectionShare share, CancellationToken cancellationToken = default);
    Task UpdateAsync(CollectionShare share, CancellationToken cancellationToken = default);
    Task DeleteAsync(CollectionShare share, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
