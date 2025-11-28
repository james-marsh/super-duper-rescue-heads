using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations;

public partial class AddSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Items",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DeletedAt",
            table: "Items",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Items_DeletedAt",
            table: "Items",
            column: "DeletedAt",
            filter: "[IsDeleted] = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Items_DeletedAt",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "Items");
    }
}
