using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Items;
using System.Text.Json;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(i => i.ItemId);

        builder.Property(i => i.ItemId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(i => i.CollectionId)
            .IsRequired();

        // Value Object: ItemName
        builder.OwnsOne(i => i.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .IsRequired()
                .HasMaxLength(200);
        });

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        // JSON column for Attributes
        builder.Property(i => i.Attributes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
            )
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.AcquisitionDate);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        // Soft Delete Properties (Feature 003)
        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.DeletedAt)
            .IsRequired(false);

        // Global Query Filter - Automatically exclude deleted items from queries
        builder.HasQueryFilter(i => !i.IsDeleted);

        // Indexes
        builder.HasIndex(i => i.CollectionId)
            .HasDatabaseName("IX_Items_CollectionId");

        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("IX_Items_CreatedAt");

        // Filtered index for purge queries - only index deleted items
        builder.HasIndex(i => i.DeletedAt)
            .HasDatabaseName("IX_Items_DeletedAt")
            .HasFilter("[IsDeleted] = 1");

        // Ignore domain events (not persisted)
        builder.Ignore(i => i.DomainEvents);
    }
}
