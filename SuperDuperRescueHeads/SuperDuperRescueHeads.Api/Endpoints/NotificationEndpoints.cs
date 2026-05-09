using Microsoft.AspNetCore.Mvc;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Notifications;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        // GET /api/v1/notifications/unread - Get unread notifications
        group.MapGet("/unread", async (
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            var notifications = await notificationService.GetUnreadNotificationsAsync(currentUserId, cancellationToken);

            return Results.Ok(new { data = notifications.Select(n => new
            {
                notificationId = n.NotificationId,
                type = n.Type.ToString(),
                priority = n.Priority.ToString(),
                title = n.Title,
                message = n.Message,
                navigationUrl = n.NavigationUrl,
                createdAt = n.CreatedAt,
                isRead = n.IsRead
            })});
        })
        .WithName("GetUnreadNotifications")
;

        // GET /api/v1/notifications - Get notification history with pagination
        group.MapGet("", async (
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50) =>
        {
            var currentUserId = currentUserService.GetUserId();

            var notifications = await notificationService.GetNotificationHistoryAsync(
                currentUserId,
                skip,
                take,
                cancellationToken);

            return Results.Ok(new
            {
                data = notifications.Select(n => new
                {
                    notificationId = n.NotificationId,
                    type = n.Type.ToString(),
                    priority = n.Priority.ToString(),
                    title = n.Title,
                    message = n.Message,
                    navigationUrl = n.NavigationUrl,
                    createdAt = n.CreatedAt,
                    isRead = n.IsRead,
                    readAt = n.ReadAt
                }),
                pagination = new
                {
                    skip,
                    take
                }
            });
        })
        .WithName("GetNotificationHistory")
;

        // PATCH /api/v1/notifications/{id}/read - Mark notification as read
        group.MapPatch("/{id:guid}/read", async (
            Guid id,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            await notificationService.MarkAsReadAsync(id, currentUserId, cancellationToken);

            return Results.Ok(new { message = "Notification marked as read" });
        })
        .WithName("MarkNotificationAsRead")
;

        // POST /api/v1/notifications/mark-all-read - Mark all notifications as read
        group.MapPost("/mark-all-read", async (
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            await notificationService.MarkAllAsReadAsync(currentUserId, cancellationToken);

            return Results.Ok(new { message = "All notifications marked as read" });
        })
        .WithName("MarkAllNotificationsAsRead")
;

        // DELETE /api/v1/notifications/{id} - Dismiss notification
        group.MapDelete("/{id:guid}", async (
            Guid id,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = currentUserService.GetUserId();

            await notificationService.DismissNotificationAsync(id, currentUserId, cancellationToken);

            return Results.NoContent();
        })
        .WithName("DismissNotification")
;

        return app;
    }
}
