# Data Model: Search Functionality

**Feature**: 004-search-functionality | **Date**: 2025-11-27

## Overview

Search functionality leverages existing Item aggregate from Feature 002 with added full-text search index. No new aggregate roots required - search operates as a read-only query layer over existing data.

## Domain Model (DDD)

### Bounded Context: Item Search & Discovery

**Purpose**: Enable users to quickly find items across collections using full-text search with filtering and relevance ranking.

**Relationship to Other Contexts**:
- **Consumes from**: Item Management context (Feature 002) - subscribes to ItemCreatedEvent, ItemUpdatedEvent, ItemRestoredEvent
- **Consumes from**: Collection Management context (Feature 001) - filters by user-owned collections
- **Consumes from**: Soft Delete context (Feature 003) - excludes soft-deleted items

### Aggregates

#### SearchQuery (Value Object / Query Object)
Represents a user's search request with all parameters.

**Properties**:
- `SearchTerm`: string (1-200 characters, required)
- `CollectionId`: Guid? (optional - null = search all collections)
- `UserId`: Guid (required - security filter)
- `Filters`: SearchFilter collection
- `Pagination`: PaginationParameters
- `Scope`: SearchScope enum (Global, CollectionSpecific)

**Validation Rules**:
- SearchTerm must be 1-200 characters
- SearchTerm cannot be only whitespace
- CollectionId must belong to UserId (if specified)
- Skip ≥ 0, Take between 1-100

**Methods**:
- `Validate()`: Ensures all rules met
- `ToSqlParameters()`: Converts to SQL query parameters

#### SearchResult (Value Object)
Represents a single item matching the search criteria.

**Properties**:
- `ItemId`: Guid
- `Name`: string
- `Notes`: string?
- `CollectionId`: Guid
- `CollectionName`: string
- `AcquisitionDate`: DateTimeOffset?
- `CreatedAt`: DateTimeOffset
- `RelevanceScore`: decimal (0.0-1.0, from SQL RANK)
- `Highlights`: string[] (matched keywords)

**Methods**:
- `FromItem(Item item, decimal rank)`: Factory method

#### SearchResultPage (Value Object)
Represents a paginated set of search results.

**Properties**:
- `Results`: SearchResult[]
- `TotalCount`: int
- `Page`: int
- `PageSize`: int
- `HasNextPage`: bool
- `HasPreviousPage`: bool
- `Query`: SearchQuery (echo back for reference)

**Methods**:
- `Create(results, totalCount, query)`: Factory method

### Value Objects

#### SearchTerm
**Properties**:
- `Value`: string (1-200 characters)

**Validation**:
- Not null, empty, or whitespace
- Max 200 characters
- Sanitize for SQL injection (parameterized queries)

**Methods**:
- `Parse(string input)`: Factory method with validation
- `ToString()`: Returns value

#### SearchFilter
**Properties**:
- `Type`: FilterType enum (AcquisitionDateRange, CustomAttribute)
- `Field`: string (attribute name if CustomAttribute type)
- `StartDate`: DateTimeOffset? (if AcquisitionDateRange)
- `EndDate`: DateTimeOffset? (if AcquisitionDateRange)
- `Value`: object? (if CustomAttribute - attribute value to match)

**Methods**:
- `ToSqlClause()`: Converts filter to SQL WHERE clause fragment

#### PaginationParameters
**Properties**:
- `Skip`: int (≥ 0, default 0)
- `Take`: int (1-100, default 20)
- `Page`: int (calculated: Skip / Take + 1)

**Methods**:
- `Create(page, pageSize)`: Factory from page number
- `NextPage()`: Returns params for next page
- `PreviousPage()`: Returns params for previous page

### Domain Services

#### ISearchService
**Purpose**: Orchestrates search queries, relevance ranking, result aggregation.

**Methods**:
```csharp
Task<SearchResultPage> SearchAsync(SearchQuery query, CancellationToken ct);
Task<string[]> GetSuggestionsAsync(string prefix, Guid userId, CancellationToken ct);
Task SaveRecentSearchAsync(Guid userId, string searchTerm, CancellationToken ct);
Task<string[]> GetRecentSearchesAsync(Guid userId, CancellationToken ct);
```

**Responsibilities**:
- Validate search query
- Call ISearchRepository for results
- Map domain objects to results
- Track recent searches

#### ISearchRepository
**Purpose**: Executes search queries against SQL Server Full-Text index.

**Methods**:
```csharp
Task<(List<Item> items, int totalCount)> SearchItemsAsync(SearchQuery query, CancellationToken ct);
Task<string[]> GetSuggestionsAsync(string prefix, Guid userId, int limit, CancellationToken ct);
```

**Implementation Notes**:
- Uses raw SQL with FREETEXTTABLE/CONTAINSTABLE for full-text search
- Returns Items with relevance scores
- Uses AsNoTracking() for read-only queries
- Projects only needed columns

## Database Schema Extensions

### Full-Text Index on Items Table

**No new tables required** - extends existing Items table from Feature 002.

**Full-Text Catalog**:
```sql
CREATE FULLTEXT CATALOG SuperDuperRescueHeadsFullTextCatalog AS DEFAULT;
```

**Full-Text Index**:
```sql
CREATE FULLTEXT INDEX ON dbo.Items
(
    Name LANGUAGE 1033,        -- English, highest priority
    Notes LANGUAGE 1033,       -- English, medium priority
    Attributes LANGUAGE 1033   -- English (JSON), lowest priority
)
KEY INDEX PK_Items
ON SuperDuperRescueHeadsFullTextCatalog
WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM);
```

**Index Columns**:
- `Name` (NVARCHAR(200)): Primary search target
- `Notes` (NVARCHAR(1000)): Secondary search target
- `Attributes` (NVARCHAR(MAX) JSON): Tertiary search target

**Change Tracking**: AUTO - index updates automatically when Items table changes

### UserSearchHistory Table (Optional - for recent searches)

**Schema**:
```sql
CREATE TABLE dbo.UserSearchHistory
(
    UserId UNIQUEIDENTIFIER NOT NULL,
    SearchTerm NVARCHAR(200) NOT NULL,
    SearchedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_UserSearchHistory PRIMARY KEY (UserId, SearchTerm),
    CONSTRAINT FK_UserSearchHistory_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IX_UserSearchHistory_SearchedAt ON dbo.UserSearchHistory(UserId, SearchedAt DESC);
```

**Purpose**: Store last 5 search terms per user for quick access

**Retention**: Keep only latest 5 searches per user (cleanup on insert)

## EF Core Configuration

### ItemSearchConfiguration (extends ItemConfiguration)

```csharp
public class ItemSearchConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        // Full-text index created via SQL migration (not EF fluent API)
        // EF Core doesn't support full-text index configuration directly

        // Ensure columns are indexed for search performance
        builder.HasIndex(i => i.CollectionId)
            .HasDatabaseName("IX_Items_CollectionId");

        builder.HasIndex(i => new { i.CollectionId, i.IsDeleted, i.CreatedAt })
            .HasDatabaseName("IX_Items_Search_Performance")
            .HasFilter("[IsDeleted] = 0");
    }
}
```

### UserSearchHistoryConfiguration

```csharp
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
            .IsDescending(false, true); // Ascending UserId, Descending SearchedAt
    }
}
```

## Enums

### SearchScope
```csharp
public enum SearchScope
{
    Global = 0,           // Search all user's collections
    CollectionSpecific = 1  // Search specific collection only
}
```

### FilterType
```csharp
public enum FilterType
{
    AcquisitionDateRange = 0,
    CustomAttribute = 1
}
```

## Validation Rules

### Search Query Validation
- ✅ SearchTerm: 1-200 characters, not whitespace-only
- ✅ CollectionId: Must belong to UserId (if specified)
- ✅ Skip: ≥ 0
- ✅ Take: 1-100
- ✅ AcquisitionDate filters: StartDate ≤ EndDate (if both specified)
- ✅ CustomAttribute filters: Field name must exist in schema

### Security Validation
- ✅ User can only search their own items (enforced via UserId filter in WHERE clause)
- ✅ Soft-deleted items excluded from results (enforced via IsDeleted = 0 filter)
- ✅ SQL injection prevented (parameterized queries only)

## Domain Events

**No new domain events** - Search subscribes to existing events from Feature 002:

### Subscriptions
- `ItemCreatedEvent`: Search index auto-updates via SQL change tracking
- `ItemUpdatedEvent`: Search index auto-updates via SQL change tracking
- `ItemRestoredEvent`: Search index auto-updates via SQL change tracking (Feature 003)
- `ItemDeletedEvent`: Search index auto-updates via SQL change tracking (item marked IsDeleted=1)

**Note**: SQL Server Full-Text index with CHANGE_TRACKING = AUTO handles index updates automatically. No manual event handlers needed for index maintenance.

## Performance Considerations

### Query Optimization
- Use `AsNoTracking()` for all search queries (read-only)
- Project only needed columns (avoid SELECT *)
- Paginate results (default 20 items/page, max 100)
- Index on (CollectionId, IsDeleted, CreatedAt) for filter performance

### Index Maintenance
- SQL Server auto-maintains full-text index (CHANGE_TRACKING = AUTO)
- Schedule weekly REORGANIZE for optimal performance
- Monitor index health via sys.dm_fts_index_keywords_by_document

### Caching Strategy
- Cache recent searches per user (5 latest) in UserSearchHistory table
- No caching of search results (data changes frequently)
- Consider Redis cache for search suggestions (future optimization)

## Example Queries

### Basic Global Search
```csharp
var query = new SearchQuery
{
    SearchTerm = "vintage camera",
    UserId = currentUserId,
    Scope = SearchScope.Global,
    Pagination = new PaginationParameters { Skip = 0, Take = 20 }
};

var results = await searchService.SearchAsync(query, cancellationToken);
```

### Collection-Specific Search with Date Filter
```csharp
var query = new SearchQuery
{
    SearchTerm = "coin",
    CollectionId = collectionId,
    UserId = currentUserId,
    Scope = SearchScope.CollectionSpecific,
    Filters = new[]
    {
        new SearchFilter
        {
            Type = FilterType.AcquisitionDateRange,
            StartDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2022, 12, 31, 23, 59, 59, TimeSpan.Zero)
        }
    },
    Pagination = new PaginationParameters { Skip = 0, Take = 20 }
};

var results = await searchService.SearchAsync(query, cancellationToken);
```

### Auto-Complete Suggestions
```csharp
var suggestions = await searchService.GetSuggestionsAsync("vint", currentUserId, cancellationToken);
// Returns: ["vintage camera", "vintage watch", "vintage coin", ...]
```

### Recent Searches
```csharp
var recentSearches = await searchService.GetRecentSearchesAsync(currentUserId, cancellationToken);
// Returns: ["coin", "vintage camera", "stamp 1920", "watch", "camera"]
```

## Migration Script (004_AddFullTextSearch.cs)

```csharp
public partial class AddFullTextSearch : Migration
{
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
```

## Summary

Search functionality extends existing Item aggregate with SQL Server Full-Text indexing. No new aggregate roots or complex domain logic - operates as a read-only query layer with relevance ranking and filtering. Simple, performant, cost-effective solution using built-in database capabilities.

**Key Points**:
- ✅ No new tables (except optional UserSearchHistory)
- ✅ Leverages existing Item aggregate from Feature 002
- ✅ SQL Server Full-Text index with auto-update
- ✅ Domain model focused on query objects and results
- ✅ Security enforced via UserId filtering
- ✅ Performance via indexing, pagination, projection
