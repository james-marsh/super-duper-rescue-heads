using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Collection entity
/// </summary>
public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(c => c.CollectionId);

        builder.Property(c => c.CollectionId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(c => c.OwnerId)
            .IsRequired();

        // Configure CollectionName value object
        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => CollectionName.Create(value))
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        // Configure ItemType value object
        builder.Property(c => c.ItemType)
            .HasConversion(
                itemType => itemType.Value,
                value => ItemType.Create(value))
            .HasColumnName("ItemType")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt);

        // Optimistic concurrency
        builder.Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes
        builder.HasIndex(c => c.OwnerId)
            .HasDatabaseName("IX_Collections_OwnerId");

        builder.HasIndex(c => c.IsDeleted)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Collections_IsDeleted");

        builder.HasIndex(c => new { c.OwnerId, c.IsDeleted })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Collections_OwnerId_IsDeleted");

        // Unique constraint: One user cannot have duplicate collection names (for non-deleted collections)
        builder.HasIndex(c => new { c.OwnerId, c.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Collections_OwnerId_Name_Unique");

        // Soft delete query filter
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Relationships
        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CollectionId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
