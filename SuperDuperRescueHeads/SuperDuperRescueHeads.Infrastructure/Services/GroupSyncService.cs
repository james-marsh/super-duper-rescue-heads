using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Services;

public class GroupSyncService : IGroupSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GroupSyncService> _logger;

    public GroupSyncService(
        ApplicationDbContext context,
        ILogger<GroupSyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SyncGroupMembershipAsync(Guid userGroupId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement external system integration
        // For now, just log that sync was requested
        _logger.LogInformation(
            "Group membership sync requested for group {UserGroupId}. External system integration not yet configured.",
            userGroupId);

        // Create sync event for tracking
        var syncEvent = GroupSyncEvent.Create(userGroupId);
        syncEvent.StartSync();

        try
        {
            // TODO: Query external user management system for current group members
            // TODO: Compare with current members in database
            // TODO: Add new members, remove departed members

            // For now, mark as completed with no changes
            syncEvent.CompleteSync(membersAdded: 0, membersRemoved: 0);

            await _context.GroupSyncEvents.AddAsync(syncEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Group membership sync completed for group {UserGroupId}",
                userGroupId);
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
