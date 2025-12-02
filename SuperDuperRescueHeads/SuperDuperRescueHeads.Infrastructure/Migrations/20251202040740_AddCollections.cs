using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.CollectionId);
                });

            migrationBuilder.CreateTable(
                name: "CollectionShares",
                columns: table => new
                {
                    CollectionShareId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharedWithUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "UserSearchHistory",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SearchTerm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SearchedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearchHistory", x => new { x.UserId, x.SearchTerm });
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcquisitionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "CollectionId",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateIndex(
                name: "IX_Collections_IsDeleted",
                table: "Collections",
                column: "IsDeleted",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId",
                table: "Collections",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId_IsDeleted",
                table: "Collections",
                columns: new[] { "OwnerId", "IsDeleted" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionShares_CollectionId_GroupId",
                table: "CollectionShares",
                columns: new[] { "CollectionId", "GroupId" },
                filter: "[GroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionShares_CollectionId_SharedWithUserId",
                table: "CollectionShares",
                columns: new[] { "CollectionId", "SharedWithUserId" },
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionShares_CollectionId_Status",
                table: "CollectionShares",
                columns: new[] { "CollectionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionShares_GroupId",
                table: "CollectionShares",
                column: "GroupId",
                filter: "[GroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionShares_InvitationToken",
                table: "CollectionShares",
                column: "InvitationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionShares_SharedWithUserId_Status",
                table: "CollectionShares",
                columns: new[] { "SharedWithUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConflictEvents_ItemId",
                table: "ConflictEvents",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictEvents_LosingUserId",
                table: "ConflictEvents",
                column: "LosingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictEvents_OccurredAt",
                table: "ConflictEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictEvents_WinningUserId",
                table: "ConflictEvents",
                column: "WinningUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserGroupId",
                table: "GroupMembers",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserGroupId_UserId",
                table: "GroupMembers",
                columns: new[] { "UserGroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSyncEvents_Status",
                table: "GroupSyncEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSyncEvents_UserGroupId_SyncStartedAt",
                table: "GroupSyncEvents",
                columns: new[] { "UserGroupId", "SyncStartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CollectionId",
                table: "Items",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedAt",
                table: "Items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DeletedAt",
                table: "Items",
                column: "DeletedAt",
                filter: "[IsDeleted] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_CreatedByUserId",
                table: "UserGroups",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_GroupName",
                table: "UserGroups",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchHistory_SearchedAt",
                table: "UserSearchHistory",
                columns: new[] { "UserId", "SearchedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionShares");

            migrationBuilder.DropTable(
                name: "ConflictEvents");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "GroupSyncEvents");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "UserSearchHistory");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "Collections");
        }
    }
}
