using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Sharing;
using SuperDuperRescueHeads.Infrastructure.Data.Entities;

namespace SuperDuperRescueHeads.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<UserSearchHistory> UserSearchHistory => Set<UserSearchHistory>();
    public DbSet<CollectionShare> CollectionShares => Set<CollectionShare>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupSyncEvent> GroupSyncEvents => Set<GroupSyncEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
