using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Repositories;

public class UserGroupRepository : IUserGroupRepository
{
    private readonly ApplicationDbContext _context;

    public UserGroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserGroup?> GetByIdAsync(Guid userGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.UserGroups
            .Include("_members")
            .AsNoTracking()
            .FirstOrDefaultAsync(ug => ug.UserGroupId == userGroupId, cancellationToken);
    }

    public async Task<List<UserGroup>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserGroups
            .Include("_members")
            .AsNoTracking()
            .Where(ug => ug.Members.Any(m => m.UserId == userId))
            .OrderBy(ug => ug.GroupName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserGroup>> GetAllAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.UserGroups
            .Include("_members")
            .AsNoTracking()
            .OrderBy(ug => ug.GroupName)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GroupMember>> GetMembersAsync(Guid userGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .Where(gm => gm.UserGroupId == userGroupId)
            .OrderBy(gm => gm.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsMemberAsync(Guid userGroupId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .AnyAsync(gm => gm.UserGroupId == userGroupId && gm.UserId == userId, cancellationToken);
    }

    public async Task<int> GetMemberCountAsync(Guid userGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AsNoTracking()
            .CountAsync(gm => gm.UserGroupId == userGroupId, cancellationToken);
    }

    public async Task AddAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        await _context.UserGroups.AddAsync(group, cancellationToken);
    }

    public Task UpdateAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        _context.UserGroups.Update(group);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        _context.UserGroups.Remove(group);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
