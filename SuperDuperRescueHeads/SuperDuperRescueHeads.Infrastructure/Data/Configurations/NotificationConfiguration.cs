using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDuperRescueHeads.Domain.Notifications;

namespace SuperDuperRescueHeads.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.NavigationUrl)
            .HasMaxLength(500);

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.IsDismissed)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        // Indexes for efficient queries
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.UserId, n.CreatedAt });
        builder.HasIndex(n => n.CreatedAt); // For cleanup jobs
    }
}
