namespace SuperDuperRescueHeads.Domain.Notifications;

/// <summary>
/// Represents a real-time event notification delivered to a user
/// </summary>
public class Notification
{
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    // Related entity references
    public Guid? CollectionId { get; private set; }
    public Guid? ItemId { get; private set; }
    public Guid? TriggeredByUserId { get; private set; }

    // Navigation target (e.g., "/collections/{id}", "/items/{id}")
    public string? NavigationUrl { get; private set; }

    // Status
    public bool IsRead { get; private set; }
    public bool IsDismissed { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? DismissedAt { get; private set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Notification() { } // For EF Core

    // Factory method for sharing notifications
    public static Notification CreateSharingNotification(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid collectionId,
        Guid triggeredByUserId,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        if (type != NotificationType.CollectionShared &&
            type != NotificationType.AccessRevoked &&
            type != NotificationType.PermissionChanged &&
            type != NotificationType.GroupAccessGranted &&
            type != NotificationType.GroupAccessRevoked)
        {
            throw new ArgumentException("Invalid notification type for sharing notification", nameof(type));
        }

        var now = DateTimeOffset.UtcNow;
        return new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            CollectionId = collectionId,
            TriggeredByUserId = triggeredByUserId,
            NavigationUrl = $"/collections/{collectionId}",
            IsRead = false,
            IsDismissed = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    // Factory method for edit notifications
    public static Notification CreateEditNotification(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid collectionId,
        Guid itemId,
        Guid triggeredByUserId,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        if (type != NotificationType.ItemAdded &&
            type != NotificationType.ItemModified &&
            type != NotificationType.ItemDeleted)
        {
            throw new ArgumentException("Invalid notification type for edit notification", nameof(type));
        }

        var now = DateTimeOffset.UtcNow;
        return new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            CollectionId = collectionId,
            ItemId = itemId,
            TriggeredByUserId = triggeredByUserId,
            NavigationUrl = $"/collections/{collectionId}/items/{itemId}",
            IsRead = false,
            IsDismissed = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    // Factory method for system notifications
    public static Notification CreateSystemNotification(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? collectionId = null,
        Guid? itemId = null,
        NotificationPriority priority = NotificationPriority.High)
    {
        if (type != NotificationType.DeletionWarning &&
            type != NotificationType.ConflictDetected)
        {
            throw new ArgumentException("Invalid notification type for system notification", nameof(type));
        }

        var now = DateTimeOffset.UtcNow;
        var navigationUrl = collectionId.HasValue && itemId.HasValue
            ? $"/collections/{collectionId}/items/{itemId}"
            : collectionId.HasValue
                ? $"/collections/{collectionId}"
                : null;

        return new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            CollectionId = collectionId,
            ItemId = itemId,
            TriggeredByUserId = null, // System notifications have no triggering user
            NavigationUrl = navigationUrl,
            IsRead = false,
            IsDismissed = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkAsUnread()
    {
        if (IsRead)
        {
            IsRead = false;
            ReadAt = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Dismiss()
    {
        if (!IsDismissed)
        {
            IsDismissed = true;
            DismissedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
