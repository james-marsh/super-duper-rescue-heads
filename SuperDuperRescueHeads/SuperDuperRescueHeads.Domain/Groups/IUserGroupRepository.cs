namespace SuperDuperRescueHeads.Domain.Groups;

public interface IUserGroupRepository
{
    Task<UserGroup?> GetByIdAsync(Guid userGroupId, CancellationToken cancellationToken = default);
    Task<List<UserGroup>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserGroup>> GetAllAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<List<GroupMember>> GetMembersAsync(Guid userGroupId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(Guid userGroupId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetMemberCountAsync(Guid userGroupId, CancellationToken cancellationToken = default);
    Task AddAsync(UserGroup group, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserGroup group, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserGroup group, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
