using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class ConflictEventConfiguration : IEntityTypeConfiguration<ConflictEvent>
{
    public void Configure(EntityTypeBuilder<ConflictEvent> builder)
    {
        builder.ToTable("ConflictEvents");

        builder.HasKey(c => c.ConflictEventId);

        builder.Property(c => c.ConflictEventId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(c => c.ItemId)
            .IsRequired();

        builder.Property(c => c.WinningUserId)
            .IsRequired();

        builder.Property(c => c.LosingUserId)
            .IsRequired();

        builder.Property(c => c.VersionAtConflict)
            .HasMaxLength(8); // RowVersion is 8 bytes

        builder.Property(c => c.ConflictResolutionMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ConflictDetails)
            .HasMaxLength(1000);

        builder.Property(c => c.NotificationSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.OccurredAt)
            .IsRequired();

        // Indexes for querying conflicts
        builder.HasIndex(c => c.ItemId)
            .HasDatabaseName("IX_ConflictEvents_ItemId");

        builder.HasIndex(c => c.WinningUserId)
            .HasDatabaseName("IX_ConflictEvents_WinningUserId");

        builder.HasIndex(c => c.LosingUserId)
            .HasDatabaseName("IX_ConflictEvents_LosingUserId");

        builder.HasIndex(c => c.OccurredAt)
            .HasDatabaseName("IX_ConflictEvents_OccurredAt");
    }
}
