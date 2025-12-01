using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations;

/// <summary>
/// Migration to add ConflictEvents table for tracking concurrent edit conflicts (Feature 009)
/// </summary>
public partial class AddConflictEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ConflictEvents",
            columns: table => new
            {
                ConflictEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WinningUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LosingUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VersionAtConflict = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
                ConflictResolutionMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ConflictDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                NotificationSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConflictEvents", x => x.ConflictEventId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ConflictEvents_ItemId",
            table: "ConflictEvents",
            column: "ItemId");

        migrationBuilder.CreateIndex(
            name: "IX_ConflictEvents_WinningUserId",
            table: "ConflictEvents",
            column: "WinningUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ConflictEvents_LosingUserId",
            table: "ConflictEvents",
            column: "LosingUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ConflictEvents_OccurredAt",
            table: "ConflictEvents",
            column: "OccurredAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ConflictEvents");
    }
}
