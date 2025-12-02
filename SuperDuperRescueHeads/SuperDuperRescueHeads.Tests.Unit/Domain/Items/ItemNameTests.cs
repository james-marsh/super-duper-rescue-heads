using SuperDuperRescueHeads.Domain.Items;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Unit.Domain.Items;

/// <summary>
/// Unit tests for ItemName value object
/// </summary>
public class ItemNameTests
{
    [Test]
    public async Task Create_WithValidName_ReturnsItemName()
    {
        // Arrange
        var validName = "Valid Item Name";

        // Act
        var itemName = ItemName.Create(validName);

        // Assert
        await Assert.That(itemName.Value).IsEqualTo(validName);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Act & Assert
        await Assert.That(() => ItemName.Create(invalidName!))
            .Throws<ArgumentException>()
            .WithMessage("Item name cannot be empty*");
    }

    [Test]
    public async Task Create_WithNameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('a', 201); // 201 characters

        // Act & Assert
        await Assert.That(() => ItemName.Create(longName))
            .Throws<ArgumentException>()
            .WithMessage("Item name cannot exceed 200 characters*");
    }

    [Test]
    public async Task Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var name1 = ItemName.Create("Test Name");
        var name2 = ItemName.Create("Test Name");

        // Act & Assert
        await Assert.That(name1.Equals(name2)).IsTrue();
        await Assert.That(name1 == name2).IsTrue();
    }

    [Test]
    public async Task Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var name1 = ItemName.Create("Name 1");
        var name2 = ItemName.Create("Name 2");

        // Act & Assert
        await Assert.That(name1.Equals(name2)).IsFalse();
        await Assert.That(name1 != name2).IsTrue();
    }

    [Test]
    public async Task GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var name1 = ItemName.Create("Test Name");
        var name2 = ItemName.Create("Test Name");

        // Act
        var hash1 = name1.GetHashCode();
        var hash2 = name2.GetHashCode();

        // Assert
        await Assert.That(hash1).IsEqualTo(hash2);
    }

    [Test]
    public async Task ToString_ReturnsValue()
    {
        // Arrange
        var itemName = ItemName.Create("Test Item");

        // Act
        var result = itemName.ToString();

        // Assert
        await Assert.That(result).IsEqualTo("Test Item");
    }
}
