# Data Model: Basic Item Management

**Feature**: 002-item-management | **Date**: 2025-11-24

## Overview

This document defines the domain model for Basic Item Management following Domain-Driven Design principles. The Item aggregate root manages individual items within collections with flexible type-specific attributes.

## Bounded Context

**Context Name**: Item Management

**Purpose**: Manages the lifecycle of items—individual things being tracked within collections (vinyl records, comic books, puzzles).

**Ubiquitous Language**:
- **Item**: An individual thing being tracked within a collection (e.g., a specific vinyl album, comic book issue)
- **Item Name**: The display name/title of an item
- **Item Attributes**: Type-specific properties (e.g., artist/album for vinyl, issue number for comics)
- **Collection**: The parent grouping that contains items (from Feature 001)

**Bounded Context Relationship**:
- **Customer-Supplier** with Collection Management context
- Items cannot exist without a Collection (collection is supplier)
- Item count aggregated back to Collection

## Aggregates

### Item (Aggregate Root)

**Purpose**: Represents an individual item being tracked within a collection with type-specific attributes.

**Identity**: `ItemId` (Guid)

**Properties**:
| Property | Type | Required | Validation | Description |
|----------|------|----------|------------|-------------|
| `ItemId` | Guid | Yes | Auto-generated | Unique identifier |
| `CollectionId` | Guid | Yes | Valid collection ID | Reference to parent Collection |
| `Name` | ItemName (Value Object) | Yes | 1-200 chars | Display name/title |
| `Notes` | string | No | Max 1000 chars | User notes/description |
| `Attributes` | Dictionary<string, object> | Yes | Max 10KB JSON | Type-specific attributes |
| `AcquisitionDate` | DateTimeOffset | No | Past or present | When item was acquired |
| `CreatedAt` | DateTimeOffset | Yes | UTC | Creation timestamp |
| `UpdatedAt` | DateTimeOffset | Yes | UTC | Last modification timestamp |

**Invariants** (enforced by aggregate):
1. Item must belong to a valid collection
2. Item name must be 1-200 characters
3. Notes max 1000 characters
4. Attributes JSON max 10KB
5. User cannot exceed 50,000 total items across all collections (checked at service layer)
6. Attributes must conform to ItemType schema (validated based on Collection's ItemType)

**Behavior** (methods):
```csharp
// Factory method for creation
public static Item Create(Guid collectionId, ItemName name, Dictionary<string, object> attributes, string? notes = null, DateTimeOffset? acquisitionDate = null)

// Update operations
public void UpdateName(ItemName newName)
public void UpdateNotes(string? newNotes)
public void UpdateAttributes(Dictionary<string, object> newAttributes)
public void UpdateAcquisitionDate(DateTimeOffset? newDate)

// Query operations
public bool BelongsToCollection(Guid collectionId)
public bool BelongsToUser(Guid userId) // checks via Collection navigation property
```

**Domain Events**:
- `ItemCreatedEvent`: Published when item is added to collection
- `ItemUpdatedEvent`: Published when item details are modified
- `ItemDeletedEvent`: Published when item is removed (hard delete in Feature 002, soft delete in Feature 003)

**Entity Diagram**:
```
┌─────────────────────────────────────────┐
│         Item (Aggregate Root)           │
├─────────────────────────────────────────┤
│ + ItemId: Guid                          │
│ + CollectionId: Guid                    │
│ + Name: ItemName <<value object>>       │
│ + Notes: string?                        │
│ + Attributes: Dictionary<string,object> │
│ + AcquisitionDate: DateTimeOffset?      │
│ + CreatedAt: DateTimeOffset             │
│ + UpdatedAt: DateTimeOffset             │
├─────────────────────────────────────────┤
│ + Create(...)                           │
│ + UpdateName(...)                       │
│ + UpdateNotes(...)                      │
│ + UpdateAttributes(...)                 │
│ + BelongsToCollection(...)              │
│ + BelongsToUser(...)                    │
└─────────────────────────────────────────┘
           │
           │ belongs to
           ↓
  ┌─────────────────┐
  │   Collection    │
  │<<aggregate root>>│
  ├─────────────────┤
  │ + CollectionId  │
  │ + OwnerId       │
  │ + ItemType      │
  │ + ItemCount     │
  └─────────────────┘
```

## Value Objects

### ItemName

**Purpose**: Encapsulates item name with validation rules.

**Properties**:
- `Value` (string): The item name, 1-200 characters, trimmed

**Invariants**:
- Cannot be null or whitespace
- Length between 1 and 200 characters
- Automatically trimmed
- Immutable once created

**Validation Rules**:
```csharp
- Value != null
- Value.Trim().Length >= 1 && Value.Trim().Length <= 200
```

**Implementation Pattern** (C# record):
```csharp
public record ItemName : ValueObject
{
    public string Value { get; }

    private ItemName(string value)
    {
        Value = value;
    }

    public static ItemName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Item name cannot be empty");

        var trimmed = value.Trim();
        if (trimmed.Length > 200)
            throw new ArgumentException("Item name cannot exceed 200 characters");

        return new ItemName(trimmed);
    }
}
```

### ItemAttributes

**Purpose**: Type-specific attributes stored as JSON, validated based on Collection's ItemType.

**Structure**: Dictionary<string, object> serialized to JSON

**Common Attributes** (all types):
- `condition` (string): Item condition (e.g., "Mint", "Good", "Fair")
- `purchasePrice` (decimal): How much user paid
- `currentValue` (decimal): Estimated current value
- `location` (string): Physical storage location

**Vinyl Record Attributes** (ItemType = 1):
```json
{
  "artist": "Pink Floyd",
  "album": "Dark Side of the Moon",
  "releaseYear": 1973,
  "label": "Harvest Records",
  "catalogNumber": "SHVL 804",
  "vinylColor": "Black",
  "vinylSize": "12 inch",
  "rpm": "33⅓",
  "condition": "Excellent",
  "pressCountry": "UK",
  "firstPressing": true
}
```

**Comic Book Attributes** (ItemType = 2):
```json
{
  "title": "The Amazing Spider-Man",
  "issueNumber": 300,
  "publisher": "Marvel Comics",
  "publicationDate": "1988-05",
  "writer": "David Michelinie",
  "artist": "Todd McFarlane",
  "coverArtist": "Todd McFarlane",
  "condition": "Near Mint",
  "gradingService": "CGC",
  "grade": 9.6,
  "variant": "Direct Edition"
}
```

**Puzzle Attributes** (ItemType = 3):
```json
{
  "title": "Starry Night",
  "manufacturer": "Ravensburger",
  "pieceCount": 1000,
  "dimensions": "27 x 20 inches",
  "artist": "Vincent van Gogh",
  "year": 2023,
  "condition": "Complete",
  "missingPieces": 0,
  "difficulty": "Intermediate"
}
```

**Validation**:
- Required attributes per type (defined in service layer)
- Data type validation (string, number, boolean, date)
- Attribute name format: alphanumeric + underscore only
- Max 10KB JSON size

## Relationships

```
User (1) ───────── owns ──────────→ (0..100) Collection
                                          │
                                          │ contains
                                          ↓
                           (0..unlimited*) Item

* unlimited per collection, but 50,000 total items per user
```

**Relationship Rules**:
1. Each Item belongs to exactly one Collection (parent)
2. A Collection can contain 0 to unlimited Items (practically: pagination required >10,000)
3. Items reference Collection by `CollectionId` (Guid), not object reference (DDD best practice)
4. Cascade delete: If collection is deleted, all its items are deleted
5. User's total item count across all collections ≤ 50,000 (enforced at service layer)

**Navigation Properties** (EF Core):
```csharp
// In Item entity
public Collection Collection { get; private set; } // EF Core navigation property

// In Collection entity (from Feature 001)
public ICollection<Item> Items { get; private set; } = new List<Item>();
```

## Database Schema (EF Core)

### Items Table

```sql
CREATE TABLE Items (
    ItemId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CollectionId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Notes NVARCHAR(1000) NULL,
    Attributes NVARCHAR(MAX) NOT NULL, -- JSON column
    AcquisitionDate DATETIMEOFFSET NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),

    CONSTRAINT FK_Items_Collections FOREIGN KEY (CollectionId)
        REFERENCES Collections(CollectionId) ON DELETE CASCADE,
    CONSTRAINT CK_Items_Attributes_JSON CHECK (ISJSON(Attributes) = 1),
    CONSTRAINT CK_Items_Attributes_Size CHECK (DATALENGTH(Attributes) <= 10240) -- 10KB
);

-- Indexes
CREATE INDEX IX_Items_CollectionId ON Items(CollectionId) INCLUDE (Name, CreatedAt);
CREATE INDEX IX_Items_CreatedAt_ItemId ON Items(CollectionId, CreatedAt DESC, ItemId DESC);
CREATE INDEX IX_Items_Name ON Items(Name);
```

**Indexes**:
- Primary key on `ItemId`
- Index on `CollectionId` (foreign key, high query frequency) with INCLUDE for list views
- Composite index on `(CollectionId, CreatedAt DESC, ItemId DESC)` for keyset pagination
- Index on `Name` for search (Feature 004 will add full-text index)

**JSON Constraints**:
- `ISJSON()` ensures valid JSON format
- `DATALENGTH()` check enforces 10KB max size

## EF Core Configuration

### Item Entity Configuration

```csharp
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(i => i.ItemId);

        // ItemName value object mapping
        builder.Property(i => i.Name)
            .HasConversion(
                name => name.Value,
                value => ItemName.Create(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasMaxLength(1000)
            .IsRequired(false);

        // JSON column for type-specific attributes
        builder.Property(i => i.Attributes)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                attrs => JsonSerializer.Serialize(attrs, JsonOptions),
                json => JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions) ?? new())
            .IsRequired();

        builder.Property(i => i.AcquisitionDate)
            .IsRequired(false);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        // Foreign key to Collection
        builder.HasOne(i => i.Collection)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.CollectionId)
            .HasDatabaseName("IX_Items_CollectionId")
            .IncludeProperties(i => new { i.Name, i.CreatedAt });

        builder.HasIndex(i => new { i.CollectionId, i.CreatedAt, i.ItemId })
            .IsDescending(false, true, true)
            .HasDatabaseName("IX_Items_Pagination");

        builder.HasIndex(i => i.Name)
            .HasDatabaseName("IX_Items_Name");

        // Ignore domain events (not persisted)
        builder.Ignore(i => i.DomainEvents);
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
}
```

## Repository Interface

```csharp
public interface IItemRepository
{
    // Queries
    Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Item>> GetByCollectionIdAsync(Guid collectionId, int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<int> CountByCollectionIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Keyset pagination (for large collections)
    Task<IReadOnlyList<Item>> GetByCollectionIdPagedAsync(Guid collectionId, DateTimeOffset? lastCreatedAt, Guid? lastItemId, int take = 100, CancellationToken cancellationToken = default);

    // Commands
    Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default);
    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);
    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);

    // Unit of Work
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## Domain Events

### ItemCreatedEvent

**Triggered When**: A new item is added to a collection.

**Payload**:
```csharp
public record ItemCreatedEvent(
    Guid ItemId,
    Guid CollectionId,
    string Name,
    DateTimeOffset CreatedAt
) : DomainEvent;
```

**Consumers**:
- Collection Management: Increment ItemCount
- Analytics service: Track item creation metrics

### ItemUpdatedEvent

**Triggered When**: Item details are modified.

**Payload**:
```csharp
public record ItemUpdatedEvent(
    Guid ItemId,
    string? OldName,
    string? NewName,
    DateTimeOffset UpdatedAt
) : DomainEvent;
```

### ItemDeletedEvent

**Triggered When**: Item is removed from collection.

**Payload**:
```csharp
public record ItemDeletedEvent(
    Guid ItemId,
    Guid CollectionId,
    DateTimeOffset DeletedAt
) : DomainEvent;
```

**Consumers**:
- Collection Management: Decrement ItemCount
- Search index: Remove item from search index (Feature 004)

## Validation Rules Summary

### Item Entity
- ✅ ItemId: Auto-generated GUID
- ✅ CollectionId: Valid collection GUID
- ✅ Name: 1-200 characters, non-empty, trimmed
- ✅ Notes: Optional, max 1000 characters
- ✅ Attributes: Valid JSON, max 10KB, type-specific schema validation
- ✅ AcquisitionDate: Optional, past or present date
- ✅ User total item limit: Max 50,000 items across all collections

### Business Rules
- ✅ Item must belong to a collection
- ✅ User can only manage items in their own collections (authorization policy)
- ✅ Collection's ItemCount automatically updated on item add/remove
- ✅ Attributes validated based on Collection's ItemType
- ✅ Hard delete in Feature 002, soft delete in Feature 003

## Data Migration Strategy

### Initial Migration (Feature 002)

**Migration Name**: `002_AddItems`

**Actions**:
1. Create `Items` table with all columns, indexes, and constraints
2. Add JSON validation constraint
3. Add size constraint on Attributes column
4. Add `Items` navigation property to `Collections` (EF Core only, no schema change)

**Rollback**:
1. Drop `Items` table
2. Remove `Items` navigation property from `Collections`

### Future Migrations

**Feature 003 (Soft Delete)**:
- Add `IsDeleted` and `DeletedAt` columns to Items
- Add query filter: `builder.HasQueryFilter(i => !i.IsDeleted)`

**Feature 004 (Search)**:
- Add full-text index on `Name` and `Attributes` JSON column
- Configure JSON paths for indexing common attributes

**Feature 005 (Custom Item Types)**:
- No schema changes (JSON column already flexible)
- Add ItemTypeSchema table (optional, for validation rules)

## Performance Considerations

### Query Optimization
- Use `AsNoTracking()` for read-only queries
- Project only needed fields in list views: `Select(i => new { i.ItemId, i.Name, i.CreatedAt })`
- Use keyset pagination for collections >1,000 items
- Include frequently queried columns in indexes (Name, CreatedAt)

### Indexing Strategy
- Index on `CollectionId` for fast item lists
- Composite index for keyset pagination
- Full-text index for name search (Feature 004)
- JSON index for attribute search (Feature 004)

### Caching Strategy (Optional)
- Cache item lists per collection for 5 minutes (Redis)
- Cache user item count for 10 minutes
- Invalidate on item create/update/delete

## Testing Data Model

### Unit Tests (Domain Layer)

**Item Aggregate Tests**:
- ✅ Create item with valid data succeeds
- ✅ Create item with invalid name throws exception
- ✅ Create item with >10KB attributes throws exception
- ✅ UpdateName with valid name succeeds and publishes event
- ✅ UpdateAttributes with valid JSON succeeds
- ✅ UpdateAttributes with invalid JSON throws exception
- ✅ BelongsToCollection returns true for correct collection
- ✅ BelongsToUser returns true for item owner

**Value Object Tests**:
- ✅ ItemName.Create with valid name succeeds
- ✅ ItemName.Create with empty name throws exception
- ✅ ItemName.Create with >200 characters throws exception
- ✅ ItemName.Create trims whitespace

### Integration Tests (Repository)

**ItemRepository Tests**:
- ✅ AddAsync persists item to database and increments collection ItemCount
- ✅ GetByIdAsync retrieves item by ID with Collection navigation property
- ✅ GetByCollectionIdAsync returns items with pagination
- ✅ GetByCollectionIdPagedAsync returns items with keyset pagination
- ✅ CountByUserIdAsync returns correct total across all collections
- ✅ UpdateAsync persists changes
- ✅ DeleteAsync removes item and decrements collection ItemCount
- ✅ Cascade delete works when collection is deleted

## Summary

The Item aggregate extends the Collection Management domain with flexible, type-specific item tracking. Key design decisions:
- **JSON column for attributes**: Avoids EAV anti-pattern, supports custom types (Feature 005)
- **Keyset pagination**: Handles large collections (10,000+ items) with O(log n) performance
- **Customer-Supplier relationship**: Items depend on Collections, ItemCount aggregated back
- **Application-managed item count**: Simple, testable, with background reconciliation

The model is designed for extensibility (soft delete in Feature 003, search in Feature 004, custom types in Feature 005) while maintaining simplicity and performance.

**Next Steps**: Create API contracts in OpenAPI format for Items endpoints.
