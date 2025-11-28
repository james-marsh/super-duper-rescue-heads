using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Infrastructure.Data.Entities;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class UserSearchHistoryConfiguration : IEntityTypeConfiguration<UserSearchHistory>
{
    public void Configure(EntityTypeBuilder<UserSearchHistory> builder)
    {
        builder.ToTable("UserSearchHistory");

        builder.HasKey(h => new { h.UserId, h.SearchTerm });

        builder.Property(h => h.SearchTerm)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.SearchedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.HasIndex(h => new { h.UserId, h.SearchedAt })
            .HasDatabaseName("IX_UserSearchHistory_SearchedAt")
            .IsDescending(false, true);
    }
}
