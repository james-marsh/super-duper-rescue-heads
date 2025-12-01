namespace SuperDuperRescueHeads.Domain.Notifications;

/// <summary>
/// Types of notifications that can be delivered to users
/// </summary>
public enum NotificationType
{
    // Sharing Events (Feature 006/007)
    CollectionShared = 0,
    AccessRevoked = 1,
    PermissionChanged = 2,
    GroupAccessGranted = 3,
    GroupAccessRevoked = 4,

    // Collaborative Edit Events (Feature 002)
    ItemAdded = 10,
    ItemModified = 11,
    ItemDeleted = 12,

    // System Events (Feature 003)
    DeletionWarning = 20,

    // Concurrent Edit Events (Feature 009)
    ConflictDetected = 30
}
