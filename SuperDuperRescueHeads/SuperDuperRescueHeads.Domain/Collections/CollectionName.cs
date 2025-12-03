namespace SuperDuperRescueHeads.Domain.Collections;

/// <summary>
/// Value object representing a collection name
/// </summary>
public record CollectionName
{
    public string Value { get; }

    private CollectionName(string value)
    {
        Value = value;
    }

    public static CollectionName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Collection name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Collection name cannot exceed 200 characters", nameof(name));

        return new CollectionName(name.Trim());
    }

    public override string ToString() => Value;
}
