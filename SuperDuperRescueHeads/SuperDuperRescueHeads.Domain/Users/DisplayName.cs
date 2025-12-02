namespace SuperDuperRescueHeads.Domain.Users;

/// <summary>
/// Value object representing a user's display name
/// </summary>
public record DisplayName
{
    public string Value { get; }

    private DisplayName(string value)
    {
        Value = value;
    }

    public static DisplayName Create(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters", nameof(displayName));

        return new DisplayName(displayName.Trim());
    }

    public override string ToString() => Value;
}
