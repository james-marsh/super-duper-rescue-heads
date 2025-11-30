namespace SuperDuperRescueHeads.Domain.Groups;

public class GroupMember
{
    public Guid GroupMemberId { get; private set; }
    public Guid UserGroupId { get; private set; }
    public Guid UserId { get; private set; }
    public GroupMemberRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private GroupMember()
    {
        // EF Core constructor
    }

    public static GroupMember Create(Guid userGroupId, Guid userId, GroupMemberRole role)
    {
        if (userGroupId == Guid.Empty)
            throw new ArgumentException("User group ID cannot be empty", nameof(userGroupId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var now = DateTimeOffset.UtcNow;

        return new GroupMember
        {
            GroupMemberId = Guid.NewGuid(),
            UserGroupId = userGroupId,
            UserId = userId,
            Role = role,
            JoinedAt = now,
            CreatedAt = now
        };
    }

    public void ChangeRole(GroupMemberRole newRole)
    {
        Role = newRole;
    }
}
