using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Groups;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers");

        builder.HasKey(gm => gm.GroupMemberId);

        builder.Property(gm => gm.UserGroupId)
            .IsRequired();

        builder.Property(gm => gm.UserId)
            .IsRequired();

        builder.Property(gm => gm.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(gm => gm.JoinedAt)
            .IsRequired();

        builder.Property(gm => gm.CreatedAt)
            .IsRequired();

        // Index for finding all groups a user belongs to
        builder.HasIndex(gm => gm.UserId);

        // Index for finding all members of a group
        builder.HasIndex(gm => gm.UserGroupId);

        // Unique constraint: user can only be in a group once
        builder.HasIndex(gm => new { gm.UserGroupId, gm.UserId })
            .IsUnique();
    }
}
