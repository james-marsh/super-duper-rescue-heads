# Data Model: Soft Delete & Recovery

**Feature**: 003-soft-delete-recovery | **Date**: 2025-11-24

## Overview

Extends Item aggregate (Feature 002) with soft delete capability.

## Item Aggregate Extensions

**New Properties**:
- `IsDeleted` (bool): Soft delete flag
- `DeletedAt` (DateTimeOffset?): When item was deleted

**New Methods**:
```csharp
public void MarkAsDeleted()
{
    IsDeleted = true;
    DeletedAt = DateTimeOffset.UtcNow;
    AddDomainEvent(new ItemDeletedEvent(ItemId, CollectionId, DeletedAt.Value));
}

public void Restore()
{
    if (!IsDeleted) throw new InvalidOperationException("Item is not deleted");
    IsDeleted = false;
    DeletedAt = null;
    UpdatedAt = DateTimeOffset.UtcNow;
    AddDomainEvent(new ItemRestoredEvent(ItemId, CollectionId, UpdatedAt));
}
```

## EF Core Configuration

```csharp
builder.Property(i => i.IsDeleted).IsRequired().HasDefaultValue(false);
builder.Property(i => i.DeletedAt).IsRequired(false);

// Global query filter
builder.HasQueryFilter(i => !i.IsDeleted);

// Filtered index for purge queries
builder.HasIndex(i => i.DeletedAt)
    .HasDatabaseName("IX_Items_DeletedAt")
    .HasFilter("[IsDeleted] = 1");
```

## Repository Extensions

```csharp
public interface IItemRepository
{
    // Existing methods still exclude deleted items (via query filter)

    // New methods for deleted items
    Task<IReadOnlyList<Item>> GetDeletedItemsAsync(Guid userId, CancellationToken ct = default);
    Task<Item?> GetDeletedItemByIdAsync(Guid itemId, CancellationToken ct = default);
    Task PermanentlyDeleteAsync(Guid itemId, CancellationToken ct = default);
    Task<int> PurgeExpiredItemsAsync(int batchSize = 1000, CancellationToken ct = default);
}

// Implementation
public async Task<IReadOnlyList<Item>> GetDeletedItemsAsync(Guid userId, CancellationToken ct)
{
    return await _context.Items
        .IgnoreQueryFilters() // Include deleted items
        .Include(i => i.Collection)
        .Where(i => i.IsDeleted && i.Collection.OwnerId == userId)
        .OrderByDescending(i => i.DeletedAt)
        .ToListAsync(ct);
}

public async Task<int> PurgeExpiredItemsAsync(int batchSize, CancellationToken ct)
{
    var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);

    var expiredItems = await _context.Items
        .IgnoreQueryFilters()
        .Where(i => i.IsDeleted && i.DeletedAt < cutoffDate)
        .Take(batchSize)
        .ToListAsync(ct);

    _context.Items.RemoveRange(expiredItems);
    return await _context.SaveChangesAsync(ct);
}
```

## Validation Rules

- Restore: Original collection must exist
- Restore: Item must be marked as deleted
- Purge: Only items deleted >30 days ago
- Manual delete from "Deleted Items": Immediate permanent deletion

## Domain Events

```csharp
public record ItemDeletedEvent(Guid ItemId, Guid CollectionId, DateTimeOffset DeletedAt) : DomainEvent;
public record ItemRestoredEvent(Guid ItemId, Guid CollectionId, DateTimeOffset RestoredAt) : DomainEvent;
public record ItemPermanentlyDeletedEvent(Guid ItemId, DateTimeOffset PurgedAt) : DomainEvent;
```
