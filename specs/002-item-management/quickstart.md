# Quickstart: Implementing Basic Item Management

**Feature**: 002-item-management | **Date**: 2025-11-24

## Overview

This quickstart extends Feature 001 (Collection Management) with item management capabilities. It follows the same TDD and DDD patterns established in Feature 001. **Prerequisite**: Complete Feature 001 first.

## Key Differences from Feature 001

- **JSON Column**: Type-specific attributes using EF Core JSON mapping
- **Pagination**: Keyset pagination for large collections (10,000+ items)
- **Relationship**: Items belong to Collections (Customer-Supplier DDD pattern)
- **Item Count**: Auto-increment/decrement Collection.ItemCount on add/remove

## Phase 1: Domain Layer (TDD)

### Step 1: Create Item Aggregate (Following Feature 001 Pattern)

**Tests First** (`SuperDuperRescueHeads.Tests.Unit/Domain/ItemTests.cs`):

```csharp
[Test]
public async Task Create_WithValidData_ShouldSucceed()
{
    // Arrange
    var collectionId = Guid.NewGuid();
    var name = ItemName.Create("Dark Side of the Moon");
    var attributes = new Dictionary<string, object>
    {
        ["artist"] = "Pink Floyd",
        ["album"] = "Dark Side of the Moon",
        ["releaseYear"] = 1973
    };

    // Act
    var item = Item.Create(collectionId, name, attributes);

    // Assert
    item.ItemId.Should().NotBeEmpty();
    item.Name.Value.Should().Be("Dark Side of the Moon");
    item.Attributes["artist"].Should().Be("Pink Floyd");
}

[Test]
public async Task Create_WithInvalidAttributes_ShouldThrow()
{
    // Arrange
    var collectionId = Guid.NewGuid();
    var name = ItemName.Create("Test Item");
    var hugeAttributes = new Dictionary<string, object>();
    for (int i = 0; i < 10000; i++) // >10KB JSON
        hugeAttributes[$"attr{i}"] = new string('x', 100);

    // Act & Assert
    Action act = () => Item.Create(collectionId, name, hugeAttributes);
    act.Should().Throw<ArgumentException>()
        .WithMessage("*Attributes cannot exceed 10KB*");
}
```

**Implementation** (`SuperDuperRescueHeads.Domain/Items/Item.cs`):

```csharp
public class Item : Entity
{
    public Guid ItemId { get; private set; }
    public Guid CollectionId { get; private set; }
    public ItemName Name { get; private set; }
    public string? Notes { get; private set; }
    public Dictionary<string, object> Attributes { get; private set; }
    public DateTimeOffset? AcquisitionDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Collection Collection { get; private set; } = null!; // EF Core navigation

    private Item() { } // EF Core

    private Item(Guid itemId, Guid collectionId, ItemName name,
        Dictionary<string, object> attributes, string? notes, DateTimeOffset? acquisitionDate)
    {
        ItemId = itemId;
        CollectionId = collectionId;
        Name = name;
        Attributes = attributes;
        Notes = notes;
        AcquisitionDate = acquisitionDate;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;

        ValidateAttributes();
    }

    public static Item Create(Guid collectionId, ItemName name,
        Dictionary<string, object> attributes, string? notes = null, DateTimeOffset? acquisitionDate = null)
    {
        var itemId = Guid.NewGuid();
        var item = new Item(itemId, collectionId, name, attributes, notes, acquisitionDate);

        item.AddDomainEvent(new ItemCreatedEvent(itemId, collectionId, name.Value, item.CreatedAt));

        return item;
    }

    private void ValidateAttributes()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(Attributes);
        if (json.Length > 10 * 1024) // 10KB
            throw new ArgumentException("Attributes cannot exceed 10KB");
    }

    public void UpdateAttributes(Dictionary<string, object> newAttributes)
    {
        Attributes = newAttributes;
        UpdatedAt = DateTimeOffset.UtcNow;
        ValidateAttributes();
    }

    public bool BelongsToCollection(Guid collectionId) => CollectionId == collectionId;
}
```

## Phase 2: Infrastructure Layer

### Step 2: EF Core Configuration with JSON Column

**Key: JSON column mapping** (`SuperDuperRescueHeads.Infrastructure/Data/Configurations/ItemConfiguration.cs`):

```csharp
builder.Property(i => i.Attributes)
    .HasColumnType("nvarchar(max)")
    .HasConversion(
        attrs => JsonSerializer.Serialize(attrs, new JsonSerializerOptions()),
        json => JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions()) ?? new())
    .IsRequired();
```

### Step 3: Repository with Item Count Management

**ItemRepository** (`SuperDuperRescueHeads.Infrastructure/Data/Repositories/ItemRepository.cs`):

```csharp
public async Task<Item> AddAsync(Item item, CancellationToken cancellationToken)
{
    await _context.Items.AddAsync(item, cancellationToken);

    // Increment collection item count
    var collection = await _context.Collections.FindAsync(item.CollectionId);
    if (collection != null)
        collection.ItemCount++;

    return item;
}

public async Task DeleteAsync(Item item, CancellationToken cancellationToken)
{
    _context.Items.Remove(item);

    // Decrement collection item count
    var collection = await _context.Collections.FindAsync(item.CollectionId);
    if (collection != null)
        collection.ItemCount--;

    return Task.CompletedTask;
}
```

### Step 4: Keyset Pagination for Large Collections

**Keyset Pagination Implementation**:

```csharp
public async Task<IReadOnlyList<Item>> GetByCollectionIdPagedAsync(
    Guid collectionId,
    DateTimeOffset? lastCreatedAt,
    Guid? lastItemId,
    int take,
    CancellationToken cancellationToken)
{
    var query = _context.Items.Where(i => i.CollectionId == collectionId);

    if (lastCreatedAt.HasValue && lastItemId.HasValue)
    {
        query = query.Where(i =>
            i.CreatedAt < lastCreatedAt.Value ||
            (i.CreatedAt == lastCreatedAt.Value && i.ItemId < lastItemId.Value));
    }

    return await query
        .OrderByDescending(i => i.CreatedAt)
        .ThenByDescending(i => i.ItemId)
        .Take(take)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}
```

## Phase 3: API Layer

### Step 5: Items Endpoints with Validation

**FluentValidation Validator**:

```csharp
public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Attributes).NotNull();

        // Vinyl-specific validation
        When(x => x.ItemTypeId == 1, () =>
        {
            RuleFor(x => x.Attributes)
                .Must(attrs => attrs.ContainsKey("artist") && attrs.ContainsKey("album"))
                .WithMessage("Vinyl records must have 'artist' and 'album' attributes");
        });
    }
}
```

**Items Endpoints** (follows Feature 001 pattern):

```csharp
group.MapGet("/{collectionId:guid}/items", async (Guid collectionId, int skip, int take, IItemRepository repo, HttpContext ctx) =>
{
    var userId = GetUserIdFromContext(ctx);

    // Verify collection ownership
    var collection = await collectionRepo.GetByIdAsync(collectionId);
    if (collection == null || !collection.IsOwnedBy(userId))
        return Results.Forbid();

    var items = await repo.GetByCollectionIdAsync(collectionId, skip, take);
    var total = await repo.CountByCollectionIdAsync(collectionId);

    return Results.Ok(new { data = items.Select(ToResponse), pagination = new { total, skip, take, hasMore = skip + take < total } });
});

group.MapPost("/{collectionId:guid}/items", async (Guid collectionId, CreateItemRequest request, IItemRepository repo, HttpContext ctx) =>
{
    var userId = GetUserIdFromContext(ctx);

    // Check collection ownership
    var collection = await collectionRepo.GetByIdAsync(collectionId);
    if (collection == null || !collection.IsOwnedBy(userId))
        return Results.Forbid();

    // Check user item limit (50,000 total)
    var userItemCount = await repo.CountByUserIdAsync(userId);
    if (userItemCount >= 50000)
        return Results.Conflict(ProblemDetailsFactory.ItemLimitExceeded(userItemCount, 50000));

    var item = Item.Create(collectionId, ItemName.Create(request.Name), request.Attributes, request.Notes, request.AcquisitionDate);
    await repo.AddAsync(item);
    await repo.SaveChangesAsync();

    return Results.Created($"/api/v1/items/{item.ItemId}", ToResponse(item));
});
```

## Phase 4: Blazor UI

### Step 6: Item List with Pagination

**Items Index Page** (`SuperDuperRescueHeads.Web/Components/Pages/Items/Index.razor`):

```razor
@page "/collections/{CollectionId:guid}/items"
@inject ItemService ItemService

<div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-6">Items in Collection</h1>

    @if (_items == null)
    {
        <p>Loading...</p>
    }
    else
    {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @foreach (var item in _items)
            {
                <ItemCard Item="@item" />
            }
        </div>

        @if (_hasMore)
        {
            <button @onclick="LoadMore" class="btn btn-secondary mt-8">Load More</button>
        }
    }
</div>

@code {
    [Parameter]
    public Guid CollectionId { get; set; }

    private List<ItemResponse>? _items;
    private int _skip = 0;
    private int _take = 100;
    private bool _hasMore;

    protected override async Task OnInitializedAsync()
    {
        await LoadItems();
    }

    private async Task LoadItems()
    {
        var response = await ItemService.GetItemsAsync(CollectionId, _skip, _take);
        _items = response.Data.ToList();
        _hasMore = response.Pagination.HasMore;
    }

    private async Task LoadMore()
    {
        _skip += _take;
        var response = await ItemService.GetItemsAsync(CollectionId, _skip, _take);
        _items!.AddRange(response.Data);
        _hasMore = response.Pagination.HasMore;
    }
}
```

### Step 7: Dynamic Attribute Fields

**ItemAttributeField Component** (handles type-specific attributes):

```razor
@typeparam TValue

<div class="form-group">
    <label class="block text-sm font-medium mb-2">@Label</label>

    @if (ValueType == typeof(string))
    {
        <input type="text" @bind="Value" class="input" />
    }
    else if (ValueType == typeof(int))
    {
        <input type="number" @bind="Value" class="input" />
    }
    else if (ValueType == typeof(DateTimeOffset) || ValueType == typeof(DateTime))
    {
        <input type="date" @bind="Value" class="input" />
    }
    else if (ValueType == typeof(bool))
    {
        <input type="checkbox" @bind="Value" class="checkbox" />
    }
</div>

@code {
    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter]
    public Type ValueType { get; set; } = typeof(string);
}
```

## Phase 5: Testing

### Step 8: Integration Tests with Large Collections

**Pagination Performance Test**:

```csharp
[Test]
public async Task GetByCollectionIdPaged_With10000Items_ShouldReturnQuickly()
{
    // Arrange
    var collectionId = Guid.NewGuid();
    for (int i = 0; i < 10000; i++)
    {
        await AddTestItem(collectionId, $"Item {i}");
    }

    var stopwatch = Stopwatch.StartNew();

    // Act
    var items = await _repository.GetByCollectionIdPagedAsync(collectionId, null, null, 100);

    stopwatch.Stop();

    // Assert
    items.Should().HaveCount(100);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(200); // P95 <200ms requirement
}
```

## Migration

```bash
# Create migration
dotnet ef migrations add AddItems --project SuperDuperRescueHeads.Infrastructure --startup-project SuperDuperRescueHeads.Api

# Update database
dotnet ef database update --project SuperDuperRescueHeads.Infrastructure --startup-project SuperDuperRescueHeads.Api
```

## Summary

Feature 002 extends Feature 001 with:
- **Item aggregate** with JSON column for flexible attributes
- **Keyset pagination** for large collections
- **Item count management** via repository
- **Type-specific validation** with FluentValidation
- **Dynamic attribute UI** in Blazor

Follow the same TDD patterns from Feature 001. All code must have tests first (RED), implementation (GREEN), refactoring.

**Next Feature**: 003-soft-delete-recovery will add `IsDeleted` flag and recovery endpoints.
