namespace SuperDuperRescueHeads.Domain.Items;

/// <summary>
/// Entity that records concurrent edit conflicts for monitoring and audit purposes
/// </summary>
public class ConflictEvent
{
    public Guid ConflictEventId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid WinningUserId { get; private set; }
    public Guid LosingUserId { get; private set; }
    public byte[]? VersionAtConflict { get; private set; }
    public string ConflictResolutionMethod { get; private set; } = null!;
    public string? ConflictDetails { get; private set; }
    public bool NotificationSent { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    // EF Core constructor
    private ConflictEvent() { }

    private ConflictEvent(
        Guid itemId,
        Guid winningUserId,
        Guid losingUserId,
        byte[]? versionAtConflict,
        string conflictResolutionMethod,
        string? conflictDetails)
    {
        ConflictEventId = Guid.NewGuid();
        ItemId = itemId;
        WinningUserId = winningUserId;
        LosingUserId = losingUserId;
        VersionAtConflict = versionAtConflict;
        ConflictResolutionMethod = conflictResolutionMethod;
        ConflictDetails = conflictDetails;
        NotificationSent = false;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public static ConflictEvent Create(
        Guid itemId,
        Guid winningUserId,
        Guid losingUserId,
        byte[]? versionAtConflict,
        string conflictDetails)
    {
        return new ConflictEvent(
            itemId,
            winningUserId,
            losingUserId,
            versionAtConflict,
            "LastWriteWins",
            conflictDetails);
    }

    public void MarkNotificationSent()
    {
        NotificationSent = true;
    }
}
