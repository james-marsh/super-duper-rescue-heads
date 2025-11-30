namespace SuperDuperRescueHeads.Domain.Groups;

public class UserGroup
{
    private readonly List<GroupMember> _members = new();

    public Guid UserGroupId { get; private set; }
    public string GroupName { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyList<GroupMember> Members => _members.AsReadOnly();

    private UserGroup()
    {
        // EF Core constructor
        GroupName = null!;
    }

    public static UserGroup Create(string groupName, string? description, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("Group name cannot be empty", nameof(groupName));

        if (groupName.Length > 100)
            throw new ArgumentException("Group name cannot exceed 100 characters", nameof(groupName));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));

        var now = DateTimeOffset.UtcNow;

        var group = new UserGroup
        {
            UserGroupId = Guid.NewGuid(),
            GroupName = groupName,
            Description = description,
            CreatedByUserId = createdByUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Creator is automatically an owner
        group.AddMember(createdByUserId, GroupMemberRole.Owner);

        return group;
    }

    public void AddMember(Guid userId, GroupMemberRole role = GroupMemberRole.Member)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        // Check if user is already a member
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException($"User {userId} is already a member of this group");

        // Validate max 50 members per group (per plan.md constraints)
        if (_members.Count >= 50)
            throw new InvalidOperationException("Group has reached maximum capacity of 50 members");

        var member = GroupMember.Create(UserGroupId, userId, role);
        _members.Add(member);

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException($"User {userId} is not a member of this group");

        // Prevent removing the last owner
        if (member.Role == GroupMemberRole.Owner && _members.Count(m => m.Role == GroupMemberRole.Owner) == 1)
            throw new InvalidOperationException("Cannot remove the last owner from the group");

        _members.Remove(member);

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeRole(Guid userId, GroupMemberRole newRole)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException($"User {userId} is not a member of this group");

        // Prevent removing the last owner
        if (member.Role == GroupMemberRole.Owner && newRole != GroupMemberRole.Owner &&
            _members.Count(m => m.Role == GroupMemberRole.Owner) == 1)
            throw new InvalidOperationException("Cannot change the role of the last owner");

        member.ChangeRole(newRole);

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDetails(string groupName, string? description)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            throw new ArgumentException("Group name cannot be empty", nameof(groupName));

        if (groupName.Length > 100)
            throw new ArgumentException("Group name cannot exceed 100 characters", nameof(groupName));

        GroupName = groupName;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    public GroupMemberRole? GetMemberRole(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId)?.Role;
    }

    public int GetMemberCount()
    {
        return _members.Count;
    }
}
