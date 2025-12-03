using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Collections;
using SuperDuperRescueHeads.Domain.Groups;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Notifications;
using SuperDuperRescueHeads.Domain.Sharing;
using SuperDuperRescueHeads.Domain.Users;
using SuperDuperRescueHeads.Infrastructure.Data.Entities;
using SuperDuperRescueHeads.Infrastructure.Identity;

namespace SuperDuperRescueHeads.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Domain entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<UserSearchHistory> UserSearchHistory => Set<UserSearchHistory>();
    public DbSet<CollectionShare> CollectionShares => Set<CollectionShare>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupSyncEvent> GroupSyncEvents => Set<GroupSyncEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ConflictEvent> ConflictEvents => Set<ConflictEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
