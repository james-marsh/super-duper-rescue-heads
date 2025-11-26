# Quickstart: Soft Delete & Recovery

**Feature**: 003-soft-delete-recovery | **Date**: 2025-11-24

## Overview

Extends Feature 002 (Items) with soft delete. **Prerequisite**: Complete Features 001-002.

## Implementation Steps

### 1. Add Properties to Item Aggregate

```csharp
public bool IsDeleted { get; private set; }
public DateTimeOffset? DeletedAt { get; private set; }

public void MarkAsDeleted()
{
    IsDeleted = true;
    DeletedAt = DateTimeOffset.UtcNow;
}

public void Restore()
{
    IsDeleted = false;
    DeletedAt = null;
}
```

### 2. Add EF Core Query Filter

```csharp
// In ItemConfiguration.cs
builder.HasQueryFilter(i => !i.IsDeleted);
```

### 3. Create Migration

```bash
dotnet ef migrations add AddSoftDelete
```

### 4. Add Hangfire for Purge Job

```bash
dotnet add SuperDuperRescueHeads.Infrastructure package Hangfire.AspNetCore
```

```csharp
// In Program.cs
builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

RecurringJob.AddOrUpdate<PurgeDeletedItemsJob>(
    "purge-deleted-items",
    job => job.ExecuteAsync(),
    Cron.Daily(2));
```

### 5. Implement Purge Job

```csharp
public class PurgeDeletedItemsJob
{
    private readonly IItemRepository _repository;

    public async Task ExecuteAsync()
    {
        var purged = await _repository.PurgeExpiredItemsAsync(batchSize: 1000);
        _logger.Information("Purged {Count} expired items", purged);
    }
}
```

### 6. Add Recovery Endpoints

```csharp
group.MapGet("/deleted-items", async (IItemRepository repo, HttpContext ctx) =>
{
    var userId = GetUserIdFromContext(ctx);
    var items = await repo.GetDeletedItemsAsync(userId);
    return Results.Ok(items.Select(ToResponse));
});

group.MapPost("/deleted-items/{id}/restore", async (Guid id, IItemRepository repo, HttpContext ctx) =>
{
    var item = await repo.GetDeletedItemByIdAsync(id);
    if (item == null) return Results.NotFound();

    item.Restore();
    await repo.UpdateAsync(item);
    await repo.SaveChangesAsync();

    return Results.Ok(ToResponse(item));
});
```

### 7. Add Deleted Items Page (Blazor)

```razor
@page "/deleted-items"
@inject ItemService ItemService

<h1>Deleted Items</h1>
<p>Items are kept for 30 days before permanent deletion.</p>

@foreach (var item in _deletedItems)
{
    <div>
        <span>@item.Name</span>
        <span>Deleted: @item.DeletedAt</span>
        <button @onclick="() => Restore(item.ItemId)">Restore</button>
    </div>
}
```

## Testing

- Unit test: `MarkAsDeleted()` sets `IsDeleted = true`
- Unit test: `Restore()` clears `IsDeleted`
- Integration test: Query filter excludes deleted items
- Integration test: `IgnoreQueryFilters()` includes deleted items
- E2E test: Delete item, restore from "Deleted Items", verify it reappears

**Feature Complete!** - Soft delete now active for all items.
