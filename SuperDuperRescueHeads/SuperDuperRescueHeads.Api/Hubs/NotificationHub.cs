using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SuperDuperRescueHeads.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly Dictionary<Guid, List<string>> _userConnections = new();
    private static readonly object _lock = new();

    public override async Task OnConnectedAsync()
    {
        // TODO: Get userId from authenticated user context
        // For now, using a placeholder - this will be updated when authentication is implemented
        var userId = Guid.Empty; // Replace with: Context.User?.FindFirst("sub")?.Value

        if (userId != Guid.Empty)
        {
            lock (_lock)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new List<string>();
                }
                _userConnections[userId].Add(Context.ConnectionId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO: Get userId from authenticated user context
        var userId = Guid.Empty;

        if (userId != Guid.Empty)
        {
            lock (_lock)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(Context.ConnectionId);
                    if (_userConnections[userId].Count == 0)
                    {
                        _userConnections.Remove(userId);
                    }
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Sends a notification to a specific user (all their active connections)
    /// </summary>
    public async Task SendNotificationToUser(Guid userId, object notification)
    {
        await Clients.Group(GetUserGroupName(userId)).SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Sends a notification to multiple users
    /// </summary>
    public async Task SendNotificationToUsers(List<Guid> userIds, object notification)
    {
        var groupNames = userIds.Select(GetUserGroupName).ToList();
        await Clients.Groups(groupNames).SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Broadcasts a read status change to all user's connections
    /// </summary>
    public async Task BroadcastNotificationRead(Guid userId, Guid notificationId)
    {
        await Clients.Group(GetUserGroupName(userId)).SendAsync("NotificationRead", notificationId);
    }

    /// <summary>
    /// Broadcasts preference changes to all user's connections
    /// </summary>
    public async Task BroadcastPreferenceChanged(Guid userId)
    {
        await Clients.Group(GetUserGroupName(userId)).SendAsync("PreferenceChanged");
    }

    /// <summary>
    /// Client calls this to send heartbeat
    /// </summary>
    public Task ReceiveHeartbeat()
    {
        // Heartbeat received - connection is alive
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets active connection count for a user
    /// </summary>
    public static int GetActiveConnectionCount(Guid userId)
    {
        lock (_lock)
        {
            return _userConnections.TryGetValue(userId, out var connections) ? connections.Count : 0;
        }
    }

    private static string GetUserGroupName(Guid userId) => $"user_{userId}";
}
