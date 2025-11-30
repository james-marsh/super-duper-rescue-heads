namespace SuperDuperRescueHeads.Domain.Groups;

public interface IGroupSyncService
{
    /// <summary>
    /// Synchronizes group membership with external user management system
    /// </summary>
    Task SyncGroupMembershipAsync(Guid userGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes all groups with external user management system
    /// </summary>
    Task SyncAllGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last sync event for a group
    /// </summary>
    Task<GroupSyncEvent?> GetLastSyncEventAsync(Guid userGroupId, CancellationToken cancellationToken = default);
}
