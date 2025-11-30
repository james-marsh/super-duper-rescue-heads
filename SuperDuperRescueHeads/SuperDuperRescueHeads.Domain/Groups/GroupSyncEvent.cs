namespace SuperDuperRescueHeads.Domain.Groups;

public class GroupSyncEvent
{
    public Guid GroupSyncEventId { get; private set; }
    public Guid UserGroupId { get; private set; }
    public GroupSyncStatus Status { get; private set; }
    public DateTimeOffset SyncStartedAt { get; private set; }
    public DateTimeOffset? SyncCompletedAt { get; private set; }
    public int MembersAdded { get; private set; }
    public int MembersRemoved { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private GroupSyncEvent()
    {
        // EF Core constructor
    }

    public static GroupSyncEvent Create(Guid userGroupId)
    {
        if (userGroupId == Guid.Empty)
            throw new ArgumentException("User group ID cannot be empty", nameof(userGroupId));

        var now = DateTimeOffset.UtcNow;

        return new GroupSyncEvent
        {
            GroupSyncEventId = Guid.NewGuid(),
            UserGroupId = userGroupId,
            Status = GroupSyncStatus.Pending,
            SyncStartedAt = now,
            MembersAdded = 0,
            MembersRemoved = 0,
            CreatedAt = now
        };
    }

    public void StartSync()
    {
        if (Status != GroupSyncStatus.Pending)
            throw new InvalidOperationException($"Cannot start sync with status {Status}");

        Status = GroupSyncStatus.InProgress;
    }

    public void CompleteSync(int membersAdded, int membersRemoved)
    {
        if (Status != GroupSyncStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete sync with status {Status}");

        Status = GroupSyncStatus.Completed;
        SyncCompletedAt = DateTimeOffset.UtcNow;
        MembersAdded = membersAdded;
        MembersRemoved = membersRemoved;
    }

    public void FailSync(string errorMessage)
    {
        if (Status == GroupSyncStatus.Completed)
            throw new InvalidOperationException("Cannot fail an already completed sync");

        Status = GroupSyncStatus.Failed;
        SyncCompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
    }
}
