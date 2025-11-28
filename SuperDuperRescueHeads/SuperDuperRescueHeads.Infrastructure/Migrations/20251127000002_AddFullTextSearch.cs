using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperDuperRescueHeads.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create full-text catalog
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'SuperDuperRescueHeadsFullTextCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG SuperDuperRescueHeadsFullTextCatalog AS DEFAULT;
                END
            ");

            // Create full-text index on Items table
            migrationBuilder.Sql(@"
                CREATE FULLTEXT INDEX ON dbo.Items
                (
                    Name LANGUAGE 1033,
                    Notes LANGUAGE 1033,
                    Attributes LANGUAGE 1033
                )
                KEY INDEX PK_Items
                ON SuperDuperRescueHeadsFullTextCatalog
                WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM);
            ");

            // Create UserSearchHistory table
            migrationBuilder.CreateTable(
                name: "UserSearchHistory",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    SearchTerm = table.Column<string>(maxLength: 200, nullable: false),
                    SearchedAt = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearchHistory", x => new { x.UserId, x.SearchTerm });
                });

            // Create index for recent searches query
            migrationBuilder.CreateIndex(
                name: "IX_UserSearchHistory_SearchedAt",
                table: "UserSearchHistory",
                columns: new[] { "UserId", "SearchedAt" },
                descending: new[] { false, true });

            // Create composite index for search performance
            migrationBuilder.CreateIndex(
                name: "IX_Items_Search_Performance",
                table: "Items",
                columns: new[] { "CollectionId", "IsDeleted", "CreatedAt" },
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop full-text index
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON dbo.Items;");

            // Drop full-text catalog
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'SuperDuperRescueHeadsFullTextCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG SuperDuperRescueHeadsFullTextCatalog;
                END
            ");

            // Drop UserSearchHistory table
            migrationBuilder.DropTable("UserSearchHistory");

            // Drop search performance index
            migrationBuilder.DropIndex("IX_Items_Search_Performance", "Items");
        }
    }
}
