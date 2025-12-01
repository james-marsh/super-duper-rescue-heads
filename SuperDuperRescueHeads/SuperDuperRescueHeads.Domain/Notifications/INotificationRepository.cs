namespace SuperDuperRescueHeads.Domain.Notifications;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetNotificationsForUserAsync(Guid userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetUnreadNotificationsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetNotificationsSinceAsync(Guid userId, DateTimeOffset since, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
