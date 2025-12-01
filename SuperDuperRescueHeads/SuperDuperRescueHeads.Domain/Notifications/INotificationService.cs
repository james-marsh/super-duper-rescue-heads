namespace SuperDuperRescueHeads.Domain.Notifications;

public interface INotificationService
{
    /// <summary>
    /// Creates a notification and sends it via SignalR to all active user connections
    /// </summary>
    Task CreateAndSendNotificationAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a batch of notifications to multiple users
    /// </summary>
    Task CreateAndSendNotificationsAsync(List<Notification> notifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread notifications for a user
    /// </summary>
    Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated notification history for a user
    /// </summary>
    Task<List<Notification>> GetNotificationHistoryAsync(Guid userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications as read for a user
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dismisses (soft deletes) a notification
    /// </summary>
    Task DismissNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}
