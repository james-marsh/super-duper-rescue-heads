# Data Model: Core Collection Management

**Feature**: 001-collection-management | **Date**: 2025-11-24

## Overview

This document defines the domain model for the Core Collection Management feature following Domain-Driven Design principles. The Collection aggregate root manages the lifecycle of user collections with enforced business invariants.

## Bounded Context

**Context Name**: Collection Management

**Purpose**: Manages the lifecycle of collections—user-created groupings for organizing items (vinyl records).

**Ubiquitous Language**:
- **Collection**: A user-created grouping with a unique name and item type
- **Collection Owner**: The user who created and owns the collection
- **Item Type**: The category of items a collection contains (e.g., "Vinyl Record", "Comic Book")
- **Collection Name**: The display name of a collection, unique per user
- **Description**: Optional text describing the collection's purpose or contents

## Aggregates

### Collection (Aggregate Root)

**Purpose**: Represents a user-created collection of items with enforced business rules.

**Identity**: `CollectionId` (Guid)

**Properties**:
| Property | Type | Required | Validation | Description |
|----------|------|----------|------------|-------------|
| `CollectionId` | Guid | Yes | Auto-generated | Unique identifier |
| `Name` | CollectionName (Value Object) | Yes | 1-100 chars, non-empty | Display name, unique per user |
| `Description` | string | No | Max 500 chars | Optional description |
| `ItemType` | ItemType (Value Object) | Yes | Predefined types | Category of items |
| `OwnerId` | Guid | Yes | Valid user ID | Reference to User (Owner) |
| `CreatedAt` | DateTimeOffset | Yes | UTC | Creation timestamp |
| `UpdatedAt` | DateTimeOffset | Yes | UTC | Last modification timestamp |
| `ItemCount` | int | Yes | ≥0 | Number of items in collection (computed) |

**Invariants** (enforced by aggregate):
1. Collection name must be unique within a user's collections
2. Collection name must be 1-100 characters, non-empty, trimmed
3. User cannot exceed 100 collections (configurable limit)
4. Owner cannot be changed after creation (immutable)
5. ItemType cannot be changed after creation (immutable)
6. Description is optional but if provided, max 500 characters
7. ItemCount must be non-negative

**Behavior** (methods):
```csharp
// Factory method for creation
public static Collection Create(CollectionName name, ItemType itemType, Guid ownerId, string? description = null)

// Update operations
public void UpdateName(CollectionName newName)
public void UpdateDescription(string? newDescription)

// Query operations
public bool IsOwnedBy(Guid userId)
public bool CanBeDeletedBy(Guid userId)
```

**Domain Events**:
- `CollectionCreated`: Published when a new collection is created
- `CollectionNameChanged`: Published when collection name is updated
- `CollectionDescriptionChanged`: Published when description is updated
- `CollectionDeleted`: Published when collection is deleted

**Entity Diagram**:
```
┌─────────────────────────────────────────┐
│         Collection (Aggregate Root)     │
├─────────────────────────────────────────┤
│ + CollectionId: Guid                    │
│ + Name: CollectionName <<value object>> │
│ + Description: string?                  │
│ + ItemType: ItemType <<value object>>   │
│ + OwnerId: Guid                         │
│ + CreatedAt: DateTimeOffset             │
│ + UpdatedAt: DateTimeOffset             │
│ + ItemCount: int                        │
├─────────────────────────────────────────┤
│ + Create(...)                           │
│ + UpdateName(...)                       │
│ + UpdateDescription(...)                │
│ + IsOwnedBy(...)                        │
│ + CanBeDeletedBy(...)                   │
└─────────────────────────────────────────┘
           │
           │ composed of
           ↓
  ┌─────────────────┐    ┌─────────────┐
  │ CollectionName  │    │  ItemType   │
  │ <<value object>>│    │<<value object>>│
  ├─────────────────┤    ├─────────────┤
  │ + Value: string │    │ + Name: str │
  ├─────────────────┤    │ + Id: int   │
  │ Validate()      │    └─────────────┘
  └─────────────────┘
```

## Value Objects

### CollectionName

**Purpose**: Encapsulates collection name with validation rules.

**Properties**:
- `Value` (string): The collection name, 1-100 characters, trimmed

**Invariants**:
- Cannot be null or whitespace
- Length between 1 and 100 characters
- Automatically trimmed
- Immutable once created

**Validation Rules**:
```csharp
- Value != null
- Value.Trim().Length >= 1 && Value.Trim().Length <= 100
```

**Equality**: Two CollectionName instances are equal if their `Value` property matches (case-sensitive).

**Implementation Pattern** (C# record):
```csharp
public record CollectionName
{
    public string Value { get; }

    private CollectionName(string value)
    {
        Value = value;
    }

    public static CollectionName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Collection name cannot be empty");

        var trimmed = value.Trim();
        if (trimmed.Length > 100)
            throw new ArgumentException("Collection name cannot exceed 100 characters");

        return new CollectionName(trimmed);
    }
}
```

### ItemType

**Purpose**: Represents the category of items a collection contains.

**Properties**:
- `Id` (int): Unique identifier for item type
- `Name` (string): Display name (e.g., "Vinyl Record", "Comic Book", "Puzzle")

**Predefined Types** (for MVP):
| Id | Name | Description |
|----|------|-------------|
| 1 | Vinyl Record | Music albums on vinyl |
| 2 | Comic Book | Comic book collection |
| 3 | Puzzle | Jigsaw puzzles |

**Invariants**:
- ItemType cannot be null
- Must be one of the predefined types
- Immutable once created

**Note**: Custom item types will be supported in Feature 005. For now, ItemType is a value object with predefined options.

**Implementation Pattern**:
```csharp
public record ItemType
{
    public int Id { get; }
    public string Name { get; }

    private ItemType(int id, string name)
    {
        Id = id;
        Name = name;
    }

    // Predefined types
    public static ItemType VinylRecord => new(1, "Vinyl Record");
    public static ItemType ComicBook => new(2, "Comic Book");
    public static ItemType Puzzle => new(3, "Puzzle");

    public static ItemType FromId(int id) => id switch
    {
        1 => VinylRecord,
        2 => ComicBook,
        3 => Puzzle,
        _ => throw new ArgumentException($"Invalid ItemType ID: {id}")
    };
}
```

## Entities

### User (External Aggregate Root)

**Purpose**: Represents a user/owner of collections. Managed by ASP.NET Core Identity.

**Identity**: `UserId` (Guid, from Identity)

**Properties** (relevant to this context):
| Property | Type | Description |
|----------|------|-------------|
| `UserId` | Guid | Unique identifier (from Identity) |
| `Email` | string | User email address |
| `UserName` | string | Display name |

**Relationship**: User owns 0..100 Collections (one-to-many).

**Note**: User aggregate is managed by the Authentication context, not Collection Management context. We only reference `UserId` as foreign key.

## Relationships

```
User (1) ───────── owns ──────────→ (0..100) Collection
     │                                    │
     │                                    │
     └─ OwnerId (FK)                      └─ Aggregate Root
```

**Relationship Rules**:
1. Each Collection has exactly one Owner (User)
2. A User can own 0 to 100 Collections (configurable limit)
3. Collections reference User by `OwnerId` (Guid), not object reference (DDD best practice)
4. Cascade delete: If user is deleted, all their collections are deleted (handled at infrastructure layer)

## Database Schema (EF Core)

### Collections Table

```sql
CREATE TABLE Collections (
    CollectionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    ItemTypeId INT NOT NULL,
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    ItemCount INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Collections_Users FOREIGN KEY (OwnerId)
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Collections_Name_OwnerId UNIQUE (Name, OwnerId),
    CONSTRAINT CK_Collections_ItemCount CHECK (ItemCount >= 0)
);

CREATE INDEX IX_Collections_OwnerId ON Collections(OwnerId);
CREATE INDEX IX_Collections_Name ON Collections(Name);
CREATE INDEX IX_Collections_CreatedAt ON Collections(CreatedAt DESC);
```

**Indexes**:
- Primary key on `CollectionId`
- Unique constraint on `(Name, OwnerId)` to enforce name uniqueness per user
- Index on `OwnerId` for efficient user collection queries
- Index on `Name` for search and duplicate detection
- Index on `CreatedAt` (DESC) for sorting collections by creation date

### ItemTypes Reference Table (Optional)

For now, ItemType is a value object with hardcoded predefined types. If we want referential integrity in database:

```sql
CREATE TABLE ItemTypes (
    ItemTypeId INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO ItemTypes (ItemTypeId, Name) VALUES
    (1, 'Vinyl Record'),
    (2, 'Comic Book'),
    (3, 'Puzzle');

ALTER TABLE Collections
    ADD CONSTRAINT FK_Collections_ItemTypes FOREIGN KEY (ItemTypeId)
        REFERENCES ItemTypes(ItemTypeId);
```

**Decision**: For MVP, skip ItemTypes table and use value object approach. Add table in Feature 005 when custom types are introduced.

## EF Core Configuration

### Collection Entity Configuration

```csharp
public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(c => c.CollectionId);

        // CollectionName value object mapping
        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => CollectionName.Create(value))
            .HasMaxLength(100)
            .IsRequired();

        // ItemType value object mapping
        builder.Property(c => c.ItemType)
            .HasConversion(
                itemType => itemType.Id,
                id => ItemType.FromId(id))
            .HasColumnName("ItemTypeId")
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.OwnerId)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.ItemCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(c => c.OwnerId)
            .HasDatabaseName("IX_Collections_OwnerId");

        builder.HasIndex(c => new { c.Name, c.OwnerId })
            .IsUnique()
            .HasDatabaseName("UQ_Collections_Name_OwnerId");

        builder.HasIndex(c => c.CreatedAt)
            .IsDescending()
            .HasDatabaseName("IX_Collections_CreatedAt");
    }
}
```

## Repository Interface

```csharp
public interface ICollectionRepository
{
    // Queries
    Task<Collection?> GetByIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Collection>> GetByOwnerIdAsync(Guid ownerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<int> CountByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndOwnerAsync(string name, Guid ownerId, CancellationToken cancellationToken = default);

    // Commands
    Task<Collection> AddAsync(Collection collection, CancellationToken cancellationToken = default);
    Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);
    Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default);

    // Unit of Work
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## Domain Events

### CollectionCreated

**Triggered When**: A new collection is successfully created.

**Payload**:
```csharp
public record CollectionCreatedEvent(
    Guid CollectionId,
    string Name,
    Guid OwnerId,
    DateTimeOffset CreatedAt
) : DomainEvent;
```

**Consumers** (future features):
- Notification service: Notify user of successful creation
- Analytics service: Track collection creation metrics

### CollectionNameChanged

**Triggered When**: Collection name is updated.

**Payload**:
```csharp
public record CollectionNameChangedEvent(
    Guid CollectionId,
    string OldName,
    string NewName,
    DateTimeOffset UpdatedAt
) : DomainEvent;
```

### CollectionDeleted

**Triggered When**: Collection is deleted.

**Payload**:
```csharp
public record CollectionDeletedEvent(
    Guid CollectionId,
    Guid OwnerId,
    DateTimeOffset DeletedAt
) : DomainEvent;
```

**Consumers** (future features):
- Item Management: Cascade delete items in collection
- Search index: Remove collection from search index

## Validation Rules Summary

### Collection Entity
- ✅ CollectionId: Auto-generated GUID
- ✅ Name: 1-100 characters, non-empty, trimmed, unique per user
- ✅ Description: Optional, max 500 characters
- ✅ ItemType: One of predefined types (1, 2, 3)
- ✅ OwnerId: Valid user GUID
- ✅ ItemCount: Non-negative integer
- ✅ User collection limit: Max 100 collections per user

### Business Rules
- ✅ Collection name must be unique within user's collections (enforced by unique index)
- ✅ User cannot exceed 100 collections (enforced in application logic before creation)
- ✅ Owner cannot be changed after creation (no setter for OwnerId)
- ✅ ItemType cannot be changed after creation (no setter for ItemType)
- ✅ Collection can only be modified/deleted by owner (enforced in authorization policy)

## Data Migration Strategy

### Initial Migration

**Migration Name**: `001_CreateCollections`

**Actions**:
1. Create `Collections` table with all columns, indexes, and constraints
2. Seed predefined ItemTypes (if using reference table approach)

**Rollback**:
1. Drop `Collections` table
2. Drop `ItemTypes` table (if created)

### Future Migrations (Feature 002+)

Feature 002 (Item Management) will add:
- `Items` table with foreign key to `Collections`
- Trigger or computed column to maintain `ItemCount`

Feature 003 (Soft Delete) will add:
- `IsDeleted` column on `Collections`
- `DeletedAt` column

## Testing Data Model

### Unit Tests (Domain Layer)

**Collection Aggregate Tests**:
- ✅ Create collection with valid data succeeds
- ✅ Create collection with invalid name throws exception
- ✅ Create collection with null ItemType throws exception
- ✅ UpdateName with valid name succeeds and publishes event
- ✅ UpdateName with invalid name throws exception
- ✅ UpdateDescription with valid description succeeds
- ✅ UpdateDescription with >500 characters throws exception
- ✅ IsOwnedBy returns true for owner, false for others
- ✅ CanBeDeletedBy returns true for owner, false for others

**Value Object Tests**:
- ✅ CollectionName.Create with valid name succeeds
- ✅ CollectionName.Create with empty/null name throws exception
- ✅ CollectionName.Create with >100 characters throws exception
- ✅ CollectionName.Create trims whitespace
- ✅ ItemType.FromId with valid ID returns correct type
- ✅ ItemType.FromId with invalid ID throws exception
- ✅ Value object equality works correctly

### Integration Tests (Repository)

**CollectionRepository Tests**:
- ✅ AddAsync persists collection to database
- ✅ GetByIdAsync retrieves collection by ID
- ✅ GetByOwnerIdAsync returns collections for owner with pagination
- ✅ CountByOwnerIdAsync returns correct count
- ✅ ExistsByNameAndOwnerAsync detects duplicate names
- ✅ UpdateAsync persists changes
- ✅ DeleteAsync removes collection
- ✅ Unique constraint on (Name, OwnerId) prevents duplicates

## Performance Considerations

### Query Optimization
- Use `AsNoTracking()` for read-only list queries
- Project only needed fields: `Select(c => new { c.Id, c.Name, c.ItemCount })`
- Paginate collection lists: default 20 per page, max 100

### Indexing Strategy
- Index on `OwnerId` for fast user collection lookups
- Unique index on `(Name, OwnerId)` for duplicate detection
- Index on `CreatedAt` for sorting

### Caching Strategy (Future)
- Cache user collection count in Redis (implement if queries exceed 200ms)
- Cache collection list per user for 5 minutes

## Summary

The data model follows Domain-Driven Design principles with Collection as the aggregate root enforcing business invariants. Value objects (CollectionName, ItemType) encapsulate validation logic. The model is simple, focused on CRUD operations, and designed for extensibility in future features (Item Management, Soft Delete, Custom Types).

**Next Steps**: Create API contracts in OpenAPI format for RESTful endpoints.
