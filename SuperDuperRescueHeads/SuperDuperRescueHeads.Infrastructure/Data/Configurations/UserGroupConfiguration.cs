using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Groups;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("UserGroups");

        builder.HasKey(ug => ug.UserGroupId);

        builder.Property(ug => ug.GroupName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ug => ug.Description)
            .HasMaxLength(500);

        builder.Property(ug => ug.CreatedByUserId)
            .IsRequired();

        builder.Property(ug => ug.CreatedAt)
            .IsRequired();

        builder.Property(ug => ug.UpdatedAt)
            .IsRequired();

        // Index for searching groups by creator
        builder.HasIndex(ug => ug.CreatedByUserId);

        // Index for group name searches
        builder.HasIndex(ug => ug.GroupName);

        // Configure the Members collection using backing field
        builder.HasMany<GroupMember>("_members")
            .WithOne()
            .HasForeignKey(gm => gm.UserGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore the read-only Members property (we're using the backing field)
        builder.Ignore(ug => ug.Members);
    }
}
