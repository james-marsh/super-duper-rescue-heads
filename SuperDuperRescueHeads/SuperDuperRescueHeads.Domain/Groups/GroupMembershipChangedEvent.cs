namespace SuperDuperRescueHeads.Domain.Groups;

public record GroupMembershipChangedEvent
{
    public required Guid UserGroupId { get; init; }
    public required string GroupName { get; init; }
    public required List<Guid> MembersAdded { get; init; }
    public required List<Guid> MembersRemoved { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public string? SyncSource { get; init; } // "Polling" or "Webhook"

    public static GroupMembershipChangedEvent Create(
        Guid userGroupId,
        string groupName,
        List<Guid> membersAdded,
        List<Guid> membersRemoved,
        string? syncSource = null)
    {
        return new GroupMembershipChangedEvent
        {
            UserGroupId = userGroupId,
            GroupName = groupName,
            MembersAdded = membersAdded,
            MembersRemoved = membersRemoved,
            OccurredAt = DateTimeOffset.UtcNow,
            SyncSource = syncSource ?? "Polling"
        };
    }

    public bool HasChanges => MembersAdded.Any() || MembersRemoved.Any();
}
