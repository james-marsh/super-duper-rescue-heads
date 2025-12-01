using Microsoft.AspNetCore.Mvc;
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
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

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
        .WithOpenApi();

        // GET /api/v1/notifications - Get notification history with pagination
        group.MapGet("", async (
            INotificationService notificationService,
            HttpContext context,
            CancellationToken cancellationToken,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

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
        .WithOpenApi();

        // PATCH /api/v1/notifications/{id}/read - Mark notification as read
        group.MapPatch("/{id:guid}/read", async (
            Guid id,
            INotificationService notificationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            await notificationService.MarkAsReadAsync(id, currentUserId, cancellationToken);

            return Results.Ok(new { message = "Notification marked as read" });
        })
        .WithName("MarkNotificationAsRead")
        .WithOpenApi();

        // POST /api/v1/notifications/mark-all-read - Mark all notifications as read
        group.MapPost("/mark-all-read", async (
            INotificationService notificationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            await notificationService.MarkAllAsReadAsync(currentUserId, cancellationToken);

            return Results.Ok(new { message = "All notifications marked as read" });
        })
        .WithName("MarkAllNotificationsAsRead")
        .WithOpenApi();

        // DELETE /api/v1/notifications/{id} - Dismiss notification
        group.MapDelete("/{id:guid}", async (
            Guid id,
            INotificationService notificationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.Empty;

            await notificationService.DismissNotificationAsync(id, currentUserId, cancellationToken);

            return Results.NoContent();
        })
        .WithName("DismissNotification")
        .WithOpenApi();

        return app;
    }
}
