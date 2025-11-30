using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class CollectionShareConfiguration : IEntityTypeConfiguration<CollectionShare>
{
    public void Configure(EntityTypeBuilder<CollectionShare> builder)
    {
        builder.ToTable("CollectionShares");

        builder.HasKey(cs => cs.CollectionShareId);

        builder.Property(cs => cs.CollectionId)
            .IsRequired();

        builder.Property(cs => cs.SharedWithUserId)
            .IsRequired();

        builder.Property(cs => cs.GroupId)
            .IsRequired(false); // Feature 007: Nullable for individual shares

        builder.Property(cs => cs.InvitedByUserId)
            .IsRequired();

        builder.Property(cs => cs.Permission)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(cs => cs.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(cs => cs.InvitationToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cs => cs.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(cs => cs.InvitedAt)
            .IsRequired();

        builder.Property(cs => cs.AcceptedAt)
            .IsRequired(false);

        builder.Property(cs => cs.ExpiresAt)
            .IsRequired();

        builder.Property(cs => cs.LastAccessedAt)
            .IsRequired(false);

        builder.Property(cs => cs.CreatedAt)
            .IsRequired();

        builder.Property(cs => cs.UpdatedAt)
            .IsRequired();

        // Index for fast token lookup
        builder.HasIndex(cs => cs.InvitationToken)
            .IsUnique();

        // Index for finding shares by collection
        builder.HasIndex(cs => new { cs.CollectionId, cs.Status });

        // Index for finding shares by user
        builder.HasIndex(cs => new { cs.SharedWithUserId, cs.Status });

        // Feature 007: Index for finding shares by group
        builder.HasIndex(cs => cs.GroupId)
            .HasFilter("[GroupId] IS NOT NULL");

        // Feature 007: Index for group shares by collection
        builder.HasIndex(cs => new { cs.CollectionId, cs.GroupId })
            .HasFilter("[GroupId] IS NOT NULL");

        // Unique constraint to prevent duplicate active shares
        builder.HasIndex(cs => new { cs.CollectionId, cs.SharedWithUserId })
            .IsUnique()
            .HasFilter("[Status] = 1"); // Only enforce for Accepted status

        // Computed property for IsGroupShare
        builder.Ignore(cs => cs.IsGroupShare);
    }
}
