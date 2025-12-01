namespace SuperDuperRescueHeads.Domain.Items;

/// <summary>
/// Service for handling concurrent edit conflicts
/// </summary>
public interface IConflictResolutionService
{
    /// <summary>
    /// Handles a concurrency conflict using last-write-wins strategy
    /// </summary>
    /// <param name="itemId">The item that had a conflict</param>
    /// <param name="winningUserId">User whose changes were saved (first to save)</param>
    /// <param name="losingUserId">User whose changes were rejected (second to save)</param>
    /// <param name="versionAtConflict">The version that was attempted to be updated</param>
    /// <param name="conflictDetails">Details about the conflict</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of conflict resolution</returns>
    Task<ConflictResolutionResult> HandleConcurrencyConflictAsync(
        Guid itemId,
        Guid winningUserId,
        Guid losingUserId,
        byte[]? versionAtConflict,
        string conflictDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a conflict event for monitoring and audit purposes
    /// </summary>
    Task RecordConflictEventAsync(
        Guid itemId,
        Guid winningUserId,
        Guid losingUserId,
        byte[]? versionAtConflict,
        string conflictDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conflict statistics for monitoring
    /// </summary>
    Task<int> GetConflictCountAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
}
