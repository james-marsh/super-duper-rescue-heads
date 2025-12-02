using Microsoft.EntityFrameworkCore;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Infrastructure.Data;
using SuperDuperRescueHeads.Infrastructure.Data.Repositories;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Integration.Infrastructure.Repositories;

/// <summary>
/// Integration tests for ItemRepository with in-memory database
/// </summary>
public class ItemRepositoryTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        return new ApplicationDbContext(options);
    }

    [Test]
    public async Task AddAsync_WithValidItem_SavesItemToDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Test Item"),
            new Dictionary<string, object> { { "Key", "Value" } },
            "Test notes");

        // Act
        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        // Assert
        var savedItem = await context.Items.FindAsync(item.ItemId);
        await Assert.That(savedItem).IsNotNull();
        await Assert.That(savedItem!.Name.Value).IsEqualTo("Test Item");
    }

    [Test]
    public async Task GetByIdAsync_WithExistingItem_ReturnsItem()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Test Item"),
            new Dictionary<string, object>());

        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(item.ItemId);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.ItemId).IsEqualTo(item.ItemId);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistentItem_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetByCollectionIdAsync_WithMatchingItems_ReturnsItems()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);
        var collectionId = Guid.NewGuid();

        var item1 = Item.Create(collectionId, ItemName.Create("Item 1"), new Dictionary<string, object>());
        var item2 = Item.Create(collectionId, ItemName.Create("Item 2"), new Dictionary<string, object>());
        var item3 = Item.Create(Guid.NewGuid(), ItemName.Create("Item 3"), new Dictionary<string, object>()); // Different collection

        await repository.AddAsync(item1);
        await repository.AddAsync(item2);
        await repository.AddAsync(item3);
        await repository.SaveChangesAsync();

        // Act
        var results = await repository.GetByCollectionIdAsync(collectionId);

        // Assert
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results.All(i => i.CollectionId == collectionId)).IsTrue();
    }

    [Test]
    public async Task UpdateAsync_WithModifiedItem_SavesChanges()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Original Name"),
            new Dictionary<string, object>());

        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        // Act
        item.UpdateName(ItemName.Create("Updated Name"));
        await repository.UpdateAsync(item);
        await repository.SaveChangesAsync();

        // Assert
        var updatedItem = await context.Items.FindAsync(item.ItemId);
        await Assert.That(updatedItem).IsNotNull();
        await Assert.That(updatedItem!.Name.Value).IsEqualTo("Updated Name");
    }

    [Test]
    public async Task DeleteAsync_SoftDeletesItem()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Test Item"),
            new Dictionary<string, object>());

        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(item);
        await repository.SaveChangesAsync();

        // Assert
        var deletedItem = await context.Items
            .IgnoreQueryFilters() // Bypass soft delete filter
            .FirstOrDefaultAsync(i => i.ItemId == item.ItemId);

        await Assert.That(deletedItem).IsNotNull();
        await Assert.That(deletedItem!.IsDeleted).IsTrue();
        await Assert.That(deletedItem.DeletedAt).IsNotNull();
    }

    [Test]
    public async Task GetByIdAsync_WithDeletedItem_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Test Item"),
            new Dictionary<string, object>());

        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        await repository.DeleteAsync(item);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(item.ItemId);

        // Assert
        await Assert.That(result).IsNull(); // Query filter excludes deleted items
    }

    [Test]
    public async Task GetDeletedItemByIdAsync_WithDeletedItem_ReturnsItem()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Test Item"),
            new Dictionary<string, object>());

        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        await repository.DeleteAsync(item);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetDeletedItemByIdAsync(item.ItemId);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.IsDeleted).IsTrue();
    }

    [Test]
    public async Task CountByCollectionIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);
        var collectionId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
        {
            var item = Item.Create(
                collectionId,
                ItemName.Create($"Item {i}"),
                new Dictionary<string, object>());
            await repository.AddAsync(item);
        }
        await repository.SaveChangesAsync();

        // Act
        var count = await repository.CountByCollectionIdAsync(collectionId);

        // Assert
        await Assert.That(count).IsEqualTo(5);
    }

    [Test]
    public async Task TryReloadAsync_WithExistingItem_ReturnsLatestVersion()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new ItemRepository(context);

        var item = Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Original"),
            new Dictionary<string, object>());

        await repository.AddAsync(item);
        await repository.SaveChangesAsync();

        // Modify the item
        item.UpdateName(ItemName.Create("Modified"));
        await repository.UpdateAsync(item);
        await repository.SaveChangesAsync();

        // Act
        var reloaded = await repository.TryReloadAsync(item.ItemId);

        // Assert
        await Assert.That(reloaded).IsNotNull();
        await Assert.That(reloaded!.Name.Value).IsEqualTo("Modified");
    }
}
