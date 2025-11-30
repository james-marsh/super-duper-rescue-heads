using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations;

public partial class AddGroupSharingSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add GroupId column to CollectionShares
        migrationBuilder.AddColumn<Guid>(
            name: "GroupId",
            table: "CollectionShares",
            type: "uniqueidentifier",
            nullable: true);

        // Index for finding shares by group
        migrationBuilder.CreateIndex(
            name: "IX_CollectionShares_GroupId",
            table: "CollectionShares",
            column: "GroupId",
            filter: "[GroupId] IS NOT NULL");

        // Index for group shares by collection
        migrationBuilder.CreateIndex(
            name: "IX_CollectionShares_CollectionId_GroupId",
            table: "CollectionShares",
            columns: new[] { "CollectionId", "GroupId" },
            filter: "[GroupId] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CollectionShares_GroupId",
            table: "CollectionShares");

        migrationBuilder.DropIndex(
            name: "IX_CollectionShares_CollectionId_GroupId",
            table: "CollectionShares");

        migrationBuilder.DropColumn(
            name: "GroupId",
            table: "CollectionShares");
    }
}
