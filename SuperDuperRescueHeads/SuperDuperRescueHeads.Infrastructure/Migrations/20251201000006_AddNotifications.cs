using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations;

public partial class AddNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                TriggeredByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                NavigationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsRead = table.Column<bool>(type: "bit", nullable: false),
                IsDismissed = table.Column<bool>(type: "bit", nullable: false),
                ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DismissedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.NotificationId);
            });

        // Indexes for efficient queries
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId",
            table: "Notifications",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId_IsRead",
            table: "Notifications",
            columns: new[] { "UserId", "IsRead" });

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId_CreatedAt",
            table: "Notifications",
            columns: new[] { "UserId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_CreatedAt",
            table: "Notifications",
            column: "CreatedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Notifications");
    }
}
