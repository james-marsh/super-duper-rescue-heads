using SuperDuperRescueHeads.Domain.Items;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Unit.Domain.Items;

/// <summary>
/// Unit tests for Item entity behavior
/// </summary>
public class ItemTests
{
    [Test]
    public async Task Create_WithValidData_ReturnsItem()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var name = ItemName.Create("Test Item");
        var attributes = new Dictionary<string, object>
        {
            { "Color", "Red" },
            { "Size", "Large" }
        };

        // Act
        var item = Item.Create(collectionId, name, attributes, "Test notes");

        // Assert
        await Assert.That(item.Name.Value).IsEqualTo("Test Item");
        await Assert.That(item.CollectionId).IsEqualTo(collectionId);
        await Assert.That(item.Notes).IsEqualTo("Test notes");
        await Assert.That(item.Attributes.Count).IsEqualTo(2);
        await Assert.That(item.IsDeleted).IsFalse();
        await Assert.That(item.DeletedAt).IsNull();
        await Assert.That(item.CreatedAt).IsNotEqualTo(default(DateTimeOffset));
    }

    [Test]
    public async Task MarkAsDeleted_WhenNotDeleted_SetsIsDeletedTrue()
    {
        // Arrange
        var item = CreateTestItem();

        // Act
        item.MarkAsDeleted();

        // Assert
        await Assert.That(item.IsDeleted).IsTrue();
        await Assert.That(item.DeletedAt).IsNotNull();
    }

    [Test]
    public async Task MarkAsDeleted_WhenAlreadyDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var item = CreateTestItem();
        item.MarkAsDeleted();

        // Act & Assert
        await Assert.That(() => item.MarkAsDeleted())
            .Throws<InvalidOperationException>()
            .WithMessage("Item is already deleted");
    }

    [Test]
    public async Task Restore_WhenDeleted_ClearsDeletedStatus()
    {
        // Arrange
        var item = CreateTestItem();
        item.MarkAsDeleted();

        // Act
        item.Restore();

        // Assert
        await Assert.That(item.IsDeleted).IsFalse();
        await Assert.That(item.DeletedAt).IsNull();
    }

    [Test]
    public async Task Restore_WhenNotDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var item = CreateTestItem();

        // Act & Assert
        await Assert.That(() => item.Restore())
            .Throws<InvalidOperationException>()
            .WithMessage("Item is not deleted");
    }

    [Test]
    public async Task UpdateName_WithNewName_UpdatesNameAndTimestamp()
    {
        // Arrange
        var item = CreateTestItem();
        var originalUpdatedAt = item.UpdatedAt;
        var newName = ItemName.Create("Updated Item Name");

        // Wait a tiny bit to ensure timestamp changes
        await Task.Delay(10);

        // Act
        item.UpdateName(newName);

        // Assert
        await Assert.That(item.Name.Value).IsEqualTo("Updated Item Name");
        await Assert.That(item.UpdatedAt).IsGreaterThan(originalUpdatedAt);
    }

    [Test]
    public async Task UpdateNotes_WithNewNotes_UpdatesNotesAndTimestamp()
    {
        // Arrange
        var item = CreateTestItem();
        var originalUpdatedAt = item.UpdatedAt;
        await Task.Delay(10);

        // Act
        item.UpdateNotes("New notes here");

        // Assert
        await Assert.That(item.Notes).IsEqualTo("New notes here");
        await Assert.That(item.UpdatedAt).IsGreaterThan(originalUpdatedAt);
    }

    [Test]
    public async Task UpdateNotes_WithTooLongNotes_ThrowsArgumentException()
    {
        // Arrange
        var item = CreateTestItem();
        var longNotes = new string('a', 1001); // 1001 characters

        // Act & Assert
        await Assert.That(() => item.UpdateNotes(longNotes))
            .Throws<ArgumentException>()
            .WithMessageMatching("Notes cannot exceed 1000 characters*");
    }

    [Test]
    public async Task UpdateAttributes_WithNewAttributes_UpdatesAttributesAndTimestamp()
    {
        // Arrange
        var item = CreateTestItem();
        var originalUpdatedAt = item.UpdatedAt;
        var newAttributes = new Dictionary<string, object>
        {
            { "Color", "Blue" },
            { "Size", "Medium" },
            { "Material", "Cotton" }
        };
        await Task.Delay(10);

        // Act
        item.UpdateAttributes(newAttributes);

        // Assert
        await Assert.That(item.Attributes.Count).IsEqualTo(3);
        await Assert.That(item.Attributes["Color"]).IsEqualTo("Blue");
        await Assert.That(item.UpdatedAt).IsGreaterThan(originalUpdatedAt);
    }

    [Test]
    public async Task BelongsToCollection_WithMatchingId_ReturnsTrue()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var item = Item.Create(
            collectionId,
            ItemName.Create("Test"),
            new Dictionary<string, object>());

        // Act
        var result = item.BelongsToCollection(collectionId);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task BelongsToCollection_WithDifferentId_ReturnsFalse()
    {
        // Arrange
        var item = CreateTestItem();
        var differentCollectionId = Guid.NewGuid();

        // Act
        var result = item.BelongsToCollection(differentCollectionId);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetCurrentVersion_ReturnsRowVersion()
    {
        // Arrange
        var item = CreateTestItem();

        // Act
        var version = item.GetCurrentVersion();

        // Assert - RowVersion is only set by EF Core, so it's null for in-memory created items
        await Assert.That(version).IsEqualTo(item.RowVersion);
    }

    // Helper method to create a test item
    private static Item CreateTestItem()
    {
        return Item.Create(
            Guid.NewGuid(),
            ItemName.Create("Test Item"),
            new Dictionary<string, object> { { "TestKey", "TestValue" } },
            "Test notes");
    }
}
