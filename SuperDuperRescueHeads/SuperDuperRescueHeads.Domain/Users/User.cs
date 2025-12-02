using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Domain.Users;

/// <summary>
/// User aggregate root representing an application user
/// </summary>
public class User
{
    public Guid UserId { get; private set; }
    public Email Email { get; private set; } = null!;
    public DisplayName DisplayName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public ICollection<Collection> OwnedCollections { get; private set; } = new List<Collection>();

    // EF Core constructor
    private User() { }

    private User(Guid userId, Email email, DisplayName displayName)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        IsActive = true;
    }

    public static User Create(Email email, DisplayName displayName)
    {
        return new User(Guid.NewGuid(), email, displayName);
    }

    public void UpdateDisplayName(DisplayName newDisplayName)
    {
        DisplayName = newDisplayName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
