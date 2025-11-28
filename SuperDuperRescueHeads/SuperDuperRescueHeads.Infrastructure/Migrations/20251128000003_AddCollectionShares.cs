using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations;

public partial class AddCollectionShares : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CollectionShares",
            columns: table => new
            {
                CollectionShareId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SharedWithUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Permission = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                InvitationToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                InvitedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                AcceptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastAccessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollectionShares", x => x.CollectionShareId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CollectionShares_InvitationToken",
            table: "CollectionShares",
            column: "InvitationToken",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CollectionShares_CollectionId_Status",
            table: "CollectionShares",
            columns: new[] { "CollectionId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_CollectionShares_SharedWithUserId_Status",
            table: "CollectionShares",
            columns: new[] { "SharedWithUserId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_CollectionShares_CollectionId_SharedWithUserId",
            table: "CollectionShares",
            columns: new[] { "CollectionId", "SharedWithUserId" },
            unique: true,
            filter: "[Status] = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CollectionShares");
    }
}
