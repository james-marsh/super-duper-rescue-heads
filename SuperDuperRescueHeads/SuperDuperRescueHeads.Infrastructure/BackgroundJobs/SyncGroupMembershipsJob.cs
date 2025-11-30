using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Groups;

namespace SuperDuperRescueHeads.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that periodically syncs group memberships with external user management system.
/// Runs every 30 seconds to meet the requirement that membership changes are reflected within 30 seconds (FR-086, SC-044, SC-045).
/// </summary>
public class SyncGroupMembershipsJob
{
    private readonly IGroupSyncService _groupSyncService;
    private readonly ILogger<SyncGroupMembershipsJob> _logger;

    public SyncGroupMembershipsJob(
        IGroupSyncService groupSyncService,
        ILogger<SyncGroupMembershipsJob> logger)
    {
        _groupSyncService = groupSyncService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting periodic group membership sync job");

            await _groupSyncService.SyncAllGroupsAsync(cancellationToken);

            _logger.LogInformation("Completed periodic group membership sync job");
        }
        catch (Exception ex)
        {
            // Log error but don't throw - Hangfire will handle retry logic
            _logger.LogError(ex, "Error occurred during group membership sync job");
        }
    }
}
