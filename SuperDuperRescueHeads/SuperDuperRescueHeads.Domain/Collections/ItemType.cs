namespace SuperDuperRescueHeads.Domain.Collections;

/// <summary>
/// Value object representing the type of items in a collection
/// </summary>
public record ItemType
{
    public string Value { get; }

    // Predefined item types
    public static readonly ItemType ComicBook = new("Comic Book");
    public static readonly ItemType Puzzle = new("Puzzle");
    public static readonly ItemType VinylRecord = new("Vinyl Record");
    public static readonly ItemType TradingCard = new("Trading Card");
    public static readonly ItemType VideoGame = new("Video Game");
    public static readonly ItemType BoardGame = new("Board Game");
    public static readonly ItemType Book = new("Book");
    public static readonly ItemType Movie = new("Movie");
    public static readonly ItemType Toy = new("Toy");
    public static readonly ItemType Collectible = new("Collectible");
    public static readonly ItemType Other = new("Other");

    // List of all predefined types for validation and UI display
    public static readonly IReadOnlyList<ItemType> PredefinedTypes = new List<ItemType>
    {
        ComicBook,
        Puzzle,
        VinylRecord,
        TradingCard,
        VideoGame,
        BoardGame,
        Book,
        Movie,
        Toy,
        Collectible,
        Other
    };

    private ItemType(string value)
    {
        Value = value;
    }

    public static ItemType Create(string itemType)
    {
        if (string.IsNullOrWhiteSpace(itemType))
            throw new ArgumentException("Item type cannot be empty", nameof(itemType));

        if (itemType.Length > 50)
            throw new ArgumentException("Item type cannot exceed 50 characters", nameof(itemType));

        var trimmed = itemType.Trim();

        // Check if it matches a predefined type (case-insensitive)
        var predefined = PredefinedTypes.FirstOrDefault(t =>
            string.Equals(t.Value, trimmed, StringComparison.OrdinalIgnoreCase));

        // Return the predefined constant if it matches, otherwise create new
        return predefined ?? new ItemType(trimmed);
    }

    public static ItemType FromValue(string value) => Create(value);

    public bool IsPredefined() => PredefinedTypes.Any(t => t.Value == Value);

    public override string ToString() => Value;
}
