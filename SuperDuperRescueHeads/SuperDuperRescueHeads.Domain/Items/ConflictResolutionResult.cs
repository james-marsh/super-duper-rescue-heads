namespace SuperDuperRescueHeads.Domain.Items;

/// <summary>
/// Value object representing the result of a conflict resolution attempt
/// </summary>
public class ConflictResolutionResult
{
    public bool WasConflict { get; }
    public bool Resolved { get; }
    public Guid? WinningUserId { get; }
    public Guid? LosingUserId { get; }
    public Item? CurrentItem { get; }
    public string? Message { get; }

    private ConflictResolutionResult(
        bool wasConflict,
        bool resolved,
        Guid? winningUserId,
        Guid? losingUserId,
        Item? currentItem,
        string? message)
    {
        WasConflict = wasConflict;
        Resolved = resolved;
        WinningUserId = winningUserId;
        LosingUserId = losingUserId;
        CurrentItem = currentItem;
        Message = message;
    }

    public static ConflictResolutionResult NoConflict() =>
        new(false, true, null, null, null, "No conflict detected");

    public static ConflictResolutionResult Conflict(
        Guid winningUserId,
        Guid losingUserId,
        Item currentItem,
        string message) =>
        new(true, true, winningUserId, losingUserId, currentItem, message);

    public static ConflictResolutionResult Unresolved(string message) =>
        new(true, false, null, null, null, message);
}
