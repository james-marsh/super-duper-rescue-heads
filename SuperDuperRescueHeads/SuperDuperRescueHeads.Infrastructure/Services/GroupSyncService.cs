using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Domain.Sharing;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Services;

public class GroupSyncService : IGroupSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GroupSyncService> _logger;
    private readonly IUserGroupRepository _groupRepository;
    private readonly ICollectionShareRepository _shareRepository;

    public GroupSyncService(
        ApplicationDbContext context,
        ILogger<GroupSyncService> logger,
        IUserGroupRepository groupRepository,
        ICollectionShareRepository shareRepository)
    {
        _context = context;
        _logger = logger;
        _groupRepository = groupRepository;
        _shareRepository = shareRepository;
    }

    public async Task SyncGroupMembershipAsync(Guid userGroupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Group membership sync requested for group {UserGroupId}",
            userGroupId);

        // Create sync event for tracking
        var syncEvent = GroupSyncEvent.Create(userGroupId);
        syncEvent.StartSync();

        try
        {
            // Get current group from database
            var group = await _groupRepository.GetByIdAsync(userGroupId, cancellationToken);
            if (group == null)
            {
                _logger.LogWarning("Group {UserGroupId} not found. Skipping sync.", userGroupId);
                return;
            }

            // Get current members from database
            var currentMembers = group.Members.Select(m => m.UserId).ToHashSet();

            // TODO: INTEGRATION POINT - Query external user management system
            // Replace this stub with actual API call to external system (e.g., Active Directory, LDAP)
            // Example:
            // var externalMembers = await _externalUserManagementClient.GetGroupMembersAsync(userGroupId);
            var externalMembers = await QueryExternalSystemForGroupMembersAsync(userGroupId, cancellationToken);

            // Compare and identify changes
            var membersToAdd = externalMembers.Except(currentMembers).ToList();
            var membersToRemove = currentMembers.Except(externalMembers).ToList();

            // Process member additions
            foreach (var userId in membersToAdd)
            {
                await ProcessMemberAddedAsync(userGroupId, userId, group.GroupName, cancellationToken);
            }

            // Process member removals
            foreach (var userId in membersToRemove)
            {
                await ProcessMemberRemovedAsync(userGroupId, userId, group.GroupName, cancellationToken);
            }

            // Mark sync as completed
            syncEvent.CompleteSync(membersAdded: membersToAdd.Count, membersRemoved: membersToRemove.Count);

            await _context.GroupSyncEvents.AddAsync(syncEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Group membership sync completed for group {UserGroupId}. Added: {MembersAdded}, Removed: {MembersRemoved}",
                userGroupId, membersToAdd.Count, membersToRemove.Count);

            // Raise domain event if there were changes
            if (membersToAdd.Any() || membersToRemove.Any())
            {
                var changeEvent = GroupMembershipChangedEvent.Create(
                    userGroupId,
                    group.GroupName,
                    membersToAdd,
                    membersToRemove,
                    "Polling");

                _logger.LogInformation(
                    "Group membership changed for {GroupName}: {MembersAdded} added, {MembersRemoved} removed",
                    group.GroupName, membersToAdd.Count, membersToRemove.Count);
            }
        }
        catch (Exception ex)
        {
            syncEvent.FailSync(ex.Message);
            await _context.GroupSyncEvents.AddAsync(syncEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex,
                "Group membership sync failed for group {UserGroupId}",
                userGroupId);
            throw;
        }
    }

    /// <summary>
    /// Handles adding a new member to a group.
    /// Automatically grants access to all collections shared with this group.
    /// </summary>
    public async Task ProcessMemberAddedAsync(
        Guid userGroupId,
        Guid userId,
        string groupName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing member addition: User {UserId} added to group {UserGroupId} ({GroupName})",
            userId, userGroupId, groupName);

        // Get the group
        var group = await _groupRepository.GetByIdAsync(userGroupId, cancellationToken);
        if (group == null)
        {
            _logger.LogWarning("Group {UserGroupId} not found", userGroupId);
            return;
        }

        // Add member to group
        try
        {
            group.AddMember(userId, GroupMemberRole.Member);
            await _groupRepository.UpdateAsync(group, cancellationToken);
            await _groupRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} successfully added to group {GroupName}. Auto-access granted to {CollectionCount} shared collections.",
                userId, groupName, 0); // Collection count will be tracked by permission system
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already a member"))
        {
            _logger.LogWarning("User {UserId} is already a member of group {GroupName}", userId, groupName);
        }
    }

    /// <summary>
    /// Handles removing a member from a group.
    /// Revokes access to collections shared with this group (unless user has individual access or access via other groups).
    /// </summary>
    public async Task ProcessMemberRemovedAsync(
        Guid userGroupId,
        Guid userId,
        string groupName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing member removal: User {UserId} removed from group {UserGroupId} ({GroupName})",
            userId, userGroupId, groupName);

        // Get the group
        var group = await _groupRepository.GetByIdAsync(userGroupId, cancellationToken);
        if (group == null)
        {
            _logger.LogWarning("Group {UserGroupId} not found", userGroupId);
            return;
        }

        // Remove member from group
        try
        {
            group.RemoveMember(userId);
            await _groupRepository.UpdateAsync(group, cancellationToken);
            await _groupRepository.SaveChangesAsync(cancellationToken);

            // Check if user still has access via other groups or individual permissions
            await CheckAndRevokeAccessAsync(userId, userGroupId, cancellationToken);

            _logger.LogInformation(
                "User {UserId} successfully removed from group {GroupName}. Access revocation processed.",
                userId, groupName);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not a member"))
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupName}", userId, groupName);
        }
    }

    /// <summary>
    /// Checks if user should lose access after being removed from a group.
    /// Only revokes access if user has no individual permission and no access via other groups.
    /// </summary>
    private async Task CheckAndRevokeAccessAsync(
        Guid userId,
        Guid removedGroupId,
        CancellationToken cancellationToken = default)
    {
        // Get all collections shared with the removed group
        var groupShares = await _shareRepository.GetByGroupIdAsync(removedGroupId, cancellationToken);

        foreach (var groupShare in groupShares)
        {
            // Check if user has individual access to this collection
            var hasIndividualAccess = await CheckForIndividualAccessAsync(
                groupShare.CollectionId,
                userId,
                cancellationToken);

            if (hasIndividualAccess)
            {
                _logger.LogInformation(
                    "User {UserId} retains access to collection {CollectionId} via individual permission",
                    userId, groupShare.CollectionId);
                continue;
            }

            // Check if user has access via other groups
            var hasOtherGroupAccess = await CheckForOtherGroupAccessAsync(
                groupShare.CollectionId,
                userId,
                removedGroupId,
                cancellationToken);

            if (hasOtherGroupAccess)
            {
                _logger.LogInformation(
                    "User {UserId} retains access to collection {CollectionId} via other group membership",
                    userId, groupShare.CollectionId);
                continue;
            }

            // User has no other access - log that access would be revoked
            // Note: Actual access revocation is handled by the authorization system
            // which checks current group memberships in real-time
            _logger.LogInformation(
                "User {UserId} lost all access to collection {CollectionId} after group removal",
                userId, groupShare.CollectionId);
        }
    }

    /// <summary>
    /// Checks if user has individual (non-group) access to a collection.
    /// </summary>
    private async Task<bool> CheckForIndividualAccessAsync(
        Guid collectionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var shares = await _shareRepository.GetByCollectionIdAsync(collectionId, cancellationToken);
        return shares.Any(s =>
            s.SharedWithUserId == userId &&
            s.Status == ShareStatus.Accepted &&
            !s.IsGroupShare);
    }

    /// <summary>
    /// Checks if user has access to a collection via other groups (excluding the removed group).
    /// </summary>
    private async Task<bool> CheckForOtherGroupAccessAsync(
        Guid collectionId,
        Guid userId,
        Guid excludedGroupId,
        CancellationToken cancellationToken = default)
    {
        // Get user's other groups
        var userGroups = await _groupRepository.GetByUserIdAsync(userId, cancellationToken);
        var otherGroupIds = userGroups
            .Where(g => g.UserGroupId != excludedGroupId)
            .Select(g => g.UserGroupId)
            .ToHashSet();

        if (!otherGroupIds.Any())
        {
            return false;
        }

        // Check if collection is shared with any of user's other groups
        var groupShares = await _shareRepository.GetGroupSharesByCollectionIdAsync(collectionId, cancellationToken);
        return groupShares.Any(s => s.GroupId.HasValue && otherGroupIds.Contains(s.GroupId.Value));
    }

    /// <summary>
    /// Stub method for querying external user management system.
    /// TODO: Replace with actual integration to external system (Active Directory, LDAP, etc.)
    /// </summary>
    private async Task<HashSet<Guid>> QueryExternalSystemForGroupMembersAsync(
        Guid userGroupId,
        CancellationToken cancellationToken = default)
    {
        // TODO: INTEGRATION POINT - Replace this stub with actual external system API call
        // Example implementations:
        // 1. Active Directory: Use System.DirectoryServices.AccountManagement
        // 2. Azure AD: Use Microsoft.Graph SDK
        // 3. Custom API: Use HttpClient to call your user management system
        // 4. LDAP: Use Novell.Directory.Ldap or similar

        _logger.LogWarning(
            "External system integration not configured. Returning empty member list for group {UserGroupId}",
            userGroupId);

        // Return empty set for now - no changes will be detected
        return await Task.FromResult(new HashSet<Guid>());
    }

    public async Task SyncAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting sync for all groups");

        var groups = await _context.UserGroups
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var group in groups)
        {
            try
            {
                await SyncGroupMembershipAsync(group.UserGroupId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to sync group {UserGroupId} ({GroupName}). Continuing with other groups.",
                    group.UserGroupId, group.GroupName);
                // Continue with other groups even if one fails
            }
        }

        _logger.LogInformation(
            "Completed sync for all groups. Total groups: {GroupCount}",
            groups.Count);
    }

    public async Task<GroupSyncEvent?> GetLastSyncEventAsync(Guid userGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupSyncEvents
            .AsNoTracking()
            .Where(gse => gse.UserGroupId == userGroupId)
            .OrderByDescending(gse => gse.SyncStartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
