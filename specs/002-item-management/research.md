# Research: Basic Item Management

**Feature**: 002-item-management | **Date**: 2025-11-24

## Overview

This document consolidates research findings for technical decisions specific to Basic Item Management. This feature builds on Feature 001 (Collection Management) and inherits most architectural decisions. This research focuses on decisions unique to item management, particularly handling flexible type-specific attributes and pagination for large datasets.

## Technology Decisions (Extending Feature 001)

### 1. Flexible Attribute Schema: JSON Column

**Decision**: Use EF Core JSON column (SQL Server NVARCHAR(MAX) with JSON validation) for type-specific item attributes

**Rationale**:
- Avoids Entity-Attribute-Value (EAV) anti-pattern which causes poor query performance
- Different item types have different attributes (comics: issueNumber, publisher; vinyl: artist, album, releaseYear)
- Feature 005 will add custom item types - schema must be extensible without migrations
- EF Core 8+ supports JSON columns with LINQ querying capabilities
- Type-safe in C# via dictionary or custom class
- Allows indexing on JSON properties for search (Feature 004)

**Alternatives Considered**:
- **EAV (Entity-Attribute-Value)**: Creates 3 tables (Entity, Attribute, Value); extremely poor query performance; complex joins; violates constitution's simplicity principle
- **Single Table Inheritance**: All item types in one table with nullable columns; schema becomes unwieldy with many types; wastes space
- **Table Per Type**: Separate table for each item type (ComicBookItems, VinylItems); requires schema migration for custom types (Feature 005); complex queries across types
- **NoSQL (Cosmos DB)**: Over-engineering; adds complexity; SQL Server sufficient for relational data; higher cost

**JSON Column Structure Example**:
```json
{
  "artist": "Pink Floyd",
  "album": "Dark Side of the Moon",
  "releaseYear": 1973,
  "condition": "Excellent",
  "vinylColor": "Black"
}
```

**EF Core Mapping**:
```csharp
builder.Property(i => i.Attributes)
    .HasColumnType("nvarchar(max)")
    .HasConversion(
        attrs => JsonSerializer.Serialize(attrs, JsonOptions),
        json => JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions));
```

**References**:
- [EF Core JSON Columns](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns)
- Martin Fowler on avoiding EAV: https://martinfowler.com/bliki/AnemicDomainModel.html

### 2. Pagination Strategy: Keyset Pagination

**Decision**: Implement keyset (seek) pagination for item lists, fallback to offset pagination for simpler cases

**Rationale**:
- Collections can have 10,000+ items (spec requirement)
- Offset pagination (`SKIP N TAKE M`) degrades with large offsets (O(n) complexity)
- Keyset pagination uses WHERE clause on indexed column (O(log n) complexity)
- Better performance for "infinite scroll" UX pattern
- Consistent results even when data changes (offset pagination can skip/duplicate rows)

**Keyset Pagination Example**:
```sql
-- First page
SELECT * FROM Items WHERE CollectionId = @id ORDER BY CreatedAt DESC, ItemId DESC LIMIT 100

-- Next page (using last item's CreatedAt and ItemId)
SELECT * FROM Items
WHERE CollectionId = @id
  AND (CreatedAt < @lastCreatedAt OR (CreatedAt = @lastCreatedAt AND ItemId < @lastItemId))
ORDER BY CreatedAt DESC, ItemId DESC
LIMIT 100
```

**When to Use Offset Pagination**:
- User explicitly navigates to page number (rare)
- Collection has <1,000 items (offset acceptable)

**Alternatives Considered**:
- **Offset Pagination Only**: Simple but poor performance for large datasets; constitution requires P95 <200ms which may fail with 10,000 items
- **Load All Items**: Violates performance requirements; excessive memory usage; poor UX for large collections

**References**:
- [Use the Index, Luke: Pagination](https://use-the-index-luke.com/no-offset)
- [EF Core Efficient Pagination](https://learn.microsoft.com/en-us/ef/core/querying/pagination)

### 3. Item Validation: FluentValidation for Complex Rules

**Decision**: Use FluentValidation library for item validation logic, extending Data Annotations

**Rationale**:
- Type-specific attributes require dynamic validation rules
- FluentValidation supports conditional rules: `When(item => item.ItemType == "Vinyl", rule => rule.RuleFor(...))`
- Cleaner than Data Annotations for complex scenarios
- Testable validation logic separate from DTOs
- Constitution mandates server-side validation; FluentValidation integrates with ASP.NET Core

**Example Validator**:
```csharp
public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        // Vinyl-specific validation
        When(x => x.ItemTypeId == 1, () =>
        {
            RuleFor(x => x.Attributes["artist"]).NotEmpty();
            RuleFor(x => x.Attributes["album"]).NotEmpty();
            RuleFor(x => x.Attributes["releaseYear"]).InclusiveBetween(1900, 2100);
        });

        // Comic-specific validation
        When(x => x.ItemTypeId == 2, () =>
        {
            RuleFor(x => x.Attributes["issueNumber"]).GreaterThan(0);
            RuleFor(x => x.Attributes["publisher"]).NotEmpty();
        });
    }
}
```

**Alternatives Considered**:
- **Data Annotations Only**: Insufficient for conditional/dynamic rules; requires custom attributes; less testable
- **Manual Validation in Domain**: Violates separation of concerns; validation logic coupled to domain model

**References**:
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- Constitution Principle IV: Input Validation

### 4. Item Count Maintenance: Database Trigger vs Application Logic

**Decision**: Maintain `ItemCount` on Collection using application logic (increment/decrement in repository), with eventual consistency check via background job

**Rationale**:
- Constitution favors application logic over database logic (testability, portability)
- Simpler to test and debug
- EF Core SaveChanges can update collection and item atomically
- Background job (Hangfire/Azure Functions) can verify counts nightly and fix discrepancies

**Implementation**:
```csharp
// In ItemRepository.AddAsync
public async Task<Item> AddAsync(Item item, CancellationToken cancellationToken)
{
    await _context.Items.AddAsync(item, cancellationToken);

    var collection = await _context.Collections.FindAsync(item.CollectionId);
    collection.ItemCount++;

    return item;
}
```

**Eventual Consistency Check** (background job):
```csharp
public async Task ReconcileItemCounts()
{
    var collections = await _context.Collections.ToListAsync();
    foreach (var collection in collections)
    {
        var actualCount = await _context.Items
            .Where(i => i.CollectionId == collection.CollectionId && !i.IsDeleted)
            .CountAsync();

        if (collection.ItemCount != actualCount)
        {
            _logger.Warning("ItemCount mismatch for {CollectionId}: expected {Expected}, actual {Actual}",
                collection.CollectionId, collection.ItemCount, actualCount);
            collection.ItemCount = actualCount;
        }
    }
    await _context.SaveChangesAsync();
}
```

**Alternatives Considered**:
- **Database Trigger**: Automatic, but harder to test; requires SQL knowledge; portability issues; violates constitution's preference for application logic
- **Computed Column**: Cannot use for collections with soft-deleted items (would need complex WHERE clause)
- **Always Query Count**: Performance penalty; defeats purpose of caching count

**References**:
- Constitution Principle VI: Simplicity & Maintainability

### 5. Soft Delete Integration (Deferred to Feature 003)

**Decision**: Hard delete items in Feature 002; soft delete mechanism will be added in Feature 003

**Rationale**:
- Feature 003 explicitly handles soft delete and recovery
- Avoids premature complexity in Feature 002
- Soft delete requires `IsDeleted` flag, `DeletedAt` timestamp, filtered queries, recovery endpoints
- Constitution YAGNI principle: implement when required

**Migration Path** (Feature 003):
1. Add `IsDeleted` and `DeletedAt` columns to Items table
2. Add global query filter: `builder.HasQueryFilter(i => !i.IsDeleted)`
3. Change DELETE endpoint to set `IsDeleted = true` instead of removing record
4. Add recovery endpoint to restore soft-deleted items
5. Background job to hard delete after 30 days

## Performance Optimizations

### Database Indexes

**Required Indexes for Feature 002**:
```sql
-- Primary key (auto-created)
PRIMARY KEY (ItemId)

-- Foreign key to Collection (high cardinality, frequently queried)
CREATE INDEX IX_Items_CollectionId ON Items(CollectionId) INCLUDE (Name, CreatedAt)

-- Pagination index (keyset pagination on CreatedAt + ItemId)
CREATE INDEX IX_Items_CreatedAt_ItemId ON Items(CollectionId, CreatedAt DESC, ItemId DESC)

-- User items count (aggregate across all collections)
CREATE INDEX IX_Items_UserId ON Items(UserId) WHERE IsDeleted = 0 (added in Feature 003)

-- Full-text search on name (Feature 004)
CREATE FULLTEXT INDEX ON Items(Name, Attributes) (added in Feature 004)
```

**Index Strategy**:
- Include frequently queried columns (Name, CreatedAt) in index to avoid table lookups
- Composite index for pagination (CollectionId + CreatedAt + ItemId)
- Filtered index for soft delete (after Feature 003)

### Caching Strategy

**Item List Caching** (optional, implement if query performance <200ms not met):
```csharp
public async Task<IReadOnlyList<Item>> GetByCollectionIdAsync(Guid collectionId, int skip, int take)
{
    var cacheKey = $"items:collection:{collectionId}:skip:{skip}:take:{take}";

    var cached = await _cache.GetAsync<List<Item>>(cacheKey);
    if (cached != null) return cached;

    var items = await _context.Items
        .Where(i => i.CollectionId == collectionId)
        .OrderByDescending(i => i.CreatedAt)
        .Skip(skip)
        .Take(take)
        .AsNoTracking()
        .ToListAsync();

    await _cache.SetAsync(cacheKey, items, TimeSpan.FromMinutes(5));

    return items;
}
```

**Cache Invalidation**:
- Invalidate on item create/update/delete
- Use Redis for distributed caching in production
- Cache TTL: 5 minutes for item lists, 1 minute for item counts

## Security Considerations

### Authorization Rules

**Item-Level Authorization**:
- Users can only manage items in their own collections
- Authorization policy: `ItemOwnerPolicy` checks `item.Collection.OwnerId == userId`
- Apply to all item endpoints: GET, POST, PUT, DELETE

**Implementation**:
```csharp
public class ItemOwnerAuthorizationHandler : AuthorizationHandler<ItemOwnerRequirement, Item>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ItemOwnerRequirement requirement,
        Item item)
    {
        var userId = context.User.FindFirst("sub")?.Value;

        if (item.Collection.OwnerId == Guid.Parse(userId))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

### Input Validation

**Attribute Validation**:
- Sanitize JSON attributes to prevent JSON injection
- Validate attribute names (alphanumeric + underscore only)
- Limit JSON size (max 10KB per item)
- Validate attribute values by type (string, number, boolean)

**SQL Injection Prevention**:
- EF Core parameterized queries automatically prevent SQL injection
- Never use raw SQL with string concatenation for JSON queries

## Cost Optimization

**Storage Costs**:
- JSON column uses NVARCHAR(MAX), charged per byte stored
- Average item JSON ~500 bytes
- 50,000 items * 500 bytes = 25MB additional storage per user (negligible)

**Query Costs**:
- Pagination reduces data transfer
- Index strategy reduces query costs (fewer scans)
- Application Insights sampling controls telemetry costs

**Data Transfer**:
- Paginate API responses (default 100 items per page, max 1000)
- Project only needed fields for list views (exclude Attributes JSON from list queries)

## Summary

Feature 002 extends Feature 001 with item management capabilities. Key technical decisions:
1. **JSON column** for flexible type-specific attributes (avoiding EAV anti-pattern)
2. **Keyset pagination** for large collections (10,000+ items)
3. **FluentValidation** for conditional validation rules
4. **Application-managed item counts** with background reconciliation
5. **Deferred soft delete** to Feature 003 (YAGNI principle)

All decisions align with constitution principles and provide a solid foundation for Features 003-005 (soft delete, search, custom types).

**Next Steps**: Proceed to Phase 1 (Design & Contracts) to create data models, API contracts, and quickstart guide.
