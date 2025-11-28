using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Items;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
