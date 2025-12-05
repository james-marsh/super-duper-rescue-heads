using SuperDuperRescueHeads.Domain.Collections;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace SuperDuperRescueHeads.Tests.Unit.Domain.Collections;

/// <summary>
/// Unit tests for ItemType value object
/// </summary>
public class ItemTypeTests
{
    [Test]
    public async Task Create_WithValidType_ReturnsItemType()
    {
        // Arrange
        var validType = "Custom Type";

        // Act
        var itemType = ItemType.Create(validType);

        // Assert
        await Assert.That(itemType.Value).IsEqualTo(validType);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_WithEmptyType_ThrowsArgumentException(string? invalidType)
    {
        // Act & Assert
        await Assert.That(() => ItemType.Create(invalidType!))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Create_WithTypeTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longType = new string('a', 51); // 51 characters

        // Act & Assert
        await Assert.That(() => ItemType.Create(longType))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Create_WithPredefinedType_ReturnsPredefinedConstant()
    {
        // Arrange
        var comicBookInput = "Comic Book";

        // Act
        var itemType = ItemType.Create(comicBookInput);

        // Assert
        await Assert.That(itemType).IsEqualTo(ItemType.ComicBook);
        await Assert.That(ReferenceEquals(itemType, ItemType.ComicBook)).IsTrue();
    }

    [Test]
    [Arguments("comic book")]
    [Arguments("COMIC BOOK")]
    [Arguments("  Comic Book  ")]
    public async Task Create_WithPredefinedTypeDifferentCasing_ReturnsPredefinedConstant(string input)
    {
        // Act
        var itemType = ItemType.Create(input);

        // Assert
        await Assert.That(itemType).IsEqualTo(ItemType.ComicBook);
        await Assert.That(ReferenceEquals(itemType, ItemType.ComicBook)).IsTrue();
    }

    [Test]
    public async Task Create_WithCustomType_ReturnsNewInstance()
    {
        // Arrange
        var customType = "My Custom Type";

        // Act
        var itemType = ItemType.Create(customType);

        // Assert
        await Assert.That(itemType.Value).IsEqualTo(customType);
        await Assert.That(itemType.IsPredefined()).IsFalse();
    }

    [Test]
    public async Task FromValue_WithValidType_ReturnsItemType()
    {
        // Arrange
        var validType = "Test Type";

        // Act
        var itemType = ItemType.FromValue(validType);

        // Assert
        await Assert.That(itemType.Value).IsEqualTo(validType);
    }

    [Test]
    public async Task IsPredefined_WithPredefinedType_ReturnsTrue()
    {
        // Arrange
        var itemType = ItemType.ComicBook;

        // Act
        var result = itemType.IsPredefined();

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsPredefined_WithCustomType_ReturnsFalse()
    {
        // Arrange
        var itemType = ItemType.Create("Custom Type");

        // Act
        var result = itemType.IsPredefined();

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PredefinedTypes_ContainsAllExpectedTypes()
    {
        // Arrange
        var expectedTypes = new[]
        {
            ItemType.ComicBook,
            ItemType.Puzzle,
            ItemType.VinylRecord,
            ItemType.TradingCard,
            ItemType.VideoGame,
            ItemType.BoardGame,
            ItemType.Book,
            ItemType.Movie,
            ItemType.Toy,
            ItemType.Collectible,
            ItemType.Other
        };

        // Assert
        await Assert.That(ItemType.PredefinedTypes.Count).IsEqualTo(11);
        foreach (var expectedType in expectedTypes)
        {
            await Assert.That(ItemType.PredefinedTypes).Contains(expectedType);
        }
    }

    [Test]
    public async Task ToString_ReturnsValue()
    {
        // Arrange
        var itemType = ItemType.Create("Test Type");

        // Act
        var result = itemType.ToString();

        // Assert
        await Assert.That(result).IsEqualTo("Test Type");
    }

    [Test]
    public async Task Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var type1 = ItemType.Create("Same Type");
        var type2 = ItemType.Create("Same Type");

        // Act & Assert
        await Assert.That(type1.Equals(type2)).IsTrue();
        await Assert.That(type1 == type2).IsTrue();
    }

    [Test]
    public async Task Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var type1 = ItemType.Create("Type 1");
        var type2 = ItemType.Create("Type 2");

        // Act & Assert
        await Assert.That(type1.Equals(type2)).IsFalse();
        await Assert.That(type1 != type2).IsTrue();
    }

    [Test]
    public async Task GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var type1 = ItemType.Create("Test Type");
        var type2 = ItemType.Create("Test Type");

        // Act
        var hash1 = type1.GetHashCode();
        var hash2 = type2.GetHashCode();

        // Assert
        await Assert.That(hash1).IsEqualTo(hash2);
    }

    [Test]
    public async Task PredefinedConstants_HaveCorrectValues()
    {
        // Assert
        await Assert.That(ItemType.ComicBook.Value).IsEqualTo("Comic Book");
        await Assert.That(ItemType.Puzzle.Value).IsEqualTo("Puzzle");
        await Assert.That(ItemType.VinylRecord.Value).IsEqualTo("Vinyl Record");
        await Assert.That(ItemType.TradingCard.Value).IsEqualTo("Trading Card");
        await Assert.That(ItemType.VideoGame.Value).IsEqualTo("Video Game");
        await Assert.That(ItemType.BoardGame.Value).IsEqualTo("Board Game");
        await Assert.That(ItemType.Book.Value).IsEqualTo("Book");
        await Assert.That(ItemType.Movie.Value).IsEqualTo("Movie");
        await Assert.That(ItemType.Toy.Value).IsEqualTo("Toy");
        await Assert.That(ItemType.Collectible.Value).IsEqualTo("Collectible");
        await Assert.That(ItemType.Other.Value).IsEqualTo("Other");
    }
}
