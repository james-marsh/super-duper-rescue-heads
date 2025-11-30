using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations;

public partial class AddUserGroups : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserGroups",
            columns: table => new
            {
                UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserGroups", x => x.UserGroupId);
            });

        migrationBuilder.CreateTable(
            name: "GroupMembers",
            columns: table => new
            {
                GroupMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Role = table.Column<int>(type: "int", nullable: false),
                JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GroupMembers", x => x.GroupMemberId);
                table.ForeignKey(
                    name: "FK_GroupMembers_UserGroups_UserGroupId",
                    column: x => x.UserGroupId,
                    principalTable: "UserGroups",
                    principalColumn: "UserGroupId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "GroupSyncEvents",
            columns: table => new
            {
                GroupSyncEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                SyncStartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                SyncCompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                MembersAdded = table.Column<int>(type: "int", nullable: false),
                MembersRemoved = table.Column<int>(type: "int", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GroupSyncEvents", x => x.GroupSyncEventId);
            });

        // Indexes for UserGroups
        migrationBuilder.CreateIndex(
            name: "IX_UserGroups_CreatedByUserId",
            table: "UserGroups",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserGroups_GroupName",
            table: "UserGroups",
            column: "GroupName");

        // Indexes for GroupMembers
        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_UserId",
            table: "GroupMembers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_UserGroupId",
            table: "GroupMembers",
            column: "UserGroupId");

        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_UserGroupId_UserId",
            table: "GroupMembers",
            columns: new[] { "UserGroupId", "UserId" },
            unique: true);

        // Indexes for GroupSyncEvents
        migrationBuilder.CreateIndex(
            name: "IX_GroupSyncEvents_UserGroupId_SyncStartedAt",
            table: "GroupSyncEvents",
            columns: new[] { "UserGroupId", "SyncStartedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_GroupSyncEvents_Status",
            table: "GroupSyncEvents",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GroupMembers");

        migrationBuilder.DropTable(
            name: "GroupSyncEvents");

        migrationBuilder.DropTable(
            name: "UserGroups");
    }
}
