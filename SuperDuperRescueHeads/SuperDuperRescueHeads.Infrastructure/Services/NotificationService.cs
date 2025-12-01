using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Notifications;

namespace SuperDuperRescueHeads.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task CreateAndSendNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        // Save notification to database
        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created notification {NotificationId} for user {UserId} of type {Type}",
            notification.NotificationId,
            notification.UserId,
            notification.Type);

        // TODO: Send real-time notification via SignalR
        // This will be implemented by injecting IHubContext in a future enhancement
    }

    public async Task CreateAndSendNotificationsAsync(
        List<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        foreach (var notification in notifications)
        {
            await _notificationRepository.AddAsync(notification, cancellationToken);
        }

        await _notificationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created {Count} notifications",
            notifications.Count);

        // TODO: Send all notifications via SignalR
        // This will be implemented in a future enhancement
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadNotificationsForUserAsync(userId, cancellationToken);
    }

    public async Task<List<Notification>> GetNotificationHistoryAsync(
        Guid userId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetNotificationsForUserAsync(userId, skip, take, cancellationToken);
    }

    public async Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification == null || notification.UserId != userId)
        {
            _logger.LogWarning(
                "Notification {NotificationId} not found or does not belong to user {UserId}",
                notificationId,
                userId);
            return;
        }

        notification.MarkAsRead();
        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        // TODO: Broadcast read status to all user's devices via SignalR
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);

        // TODO: Broadcast to all user's devices via SignalR
    }

    public async Task DismissNotificationAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification == null || notification.UserId != userId)
        {
            _logger.LogWarning(
                "Notification {NotificationId} not found or does not belong to user {UserId}",
                notificationId,
                userId);
            return;
        }

        notification.Dismiss();
        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}
