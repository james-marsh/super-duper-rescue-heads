using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Users;
using SuperDuperRescueHeads.Infrastructure.Data;

namespace SuperDuperRescueHeads.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.OwnedCollections)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.OwnedCollections)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(u => u.UserId == userId, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
