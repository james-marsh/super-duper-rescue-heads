namespace SuperDuperRescueHeads.Domain.Items;

public record ItemName
{
    public string Value { get; }

    private ItemName(string value)
    {
        Value = value;
    }

    public static ItemName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Item name cannot be empty", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > 200)
            throw new ArgumentException("Item name cannot exceed 200 characters", nameof(value));

        return new ItemName(trimmed);
    }

    public override string ToString() => Value;
}
