using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Groups;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class GroupSyncEventConfiguration : IEntityTypeConfiguration<GroupSyncEvent>
{
    public void Configure(EntityTypeBuilder<GroupSyncEvent> builder)
    {
        builder.ToTable("GroupSyncEvents");

        builder.HasKey(gse => gse.GroupSyncEventId);

        builder.Property(gse => gse.UserGroupId)
            .IsRequired();

        builder.Property(gse => gse.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(gse => gse.SyncStartedAt)
            .IsRequired();

        builder.Property(gse => gse.SyncCompletedAt)
            .IsRequired(false);

        builder.Property(gse => gse.MembersAdded)
            .IsRequired();

        builder.Property(gse => gse.MembersRemoved)
            .IsRequired();

        builder.Property(gse => gse.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(gse => gse.CreatedAt)
            .IsRequired();

        // Index for finding sync events by group
        builder.HasIndex(gse => new { gse.UserGroupId, gse.SyncStartedAt });

        // Index for finding failed syncs
        builder.HasIndex(gse => gse.Status);
    }
}
