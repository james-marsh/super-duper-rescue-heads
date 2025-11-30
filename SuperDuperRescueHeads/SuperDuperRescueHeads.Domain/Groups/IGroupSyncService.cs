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

    /// <summary>
    /// Processes a member being added to a group (grants collection access)
    /// </summary>
    Task ProcessMemberAddedAsync(Guid userGroupId, Guid userId, string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a member being removed from a group (revokes collection access if no other access exists)
    /// </summary>
    Task ProcessMemberRemovedAsync(Guid userGroupId, Guid userId, string groupName, CancellationToken cancellationToken = default);
}
