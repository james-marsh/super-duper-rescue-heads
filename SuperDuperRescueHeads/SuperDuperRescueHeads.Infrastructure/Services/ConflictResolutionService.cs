using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Items;
using SuperDuperRescueHeads.Domain.Notifications;

namespace SuperDuperRescueHeads.Infrastructure.Services;

public class ConflictResolutionService : IConflictResolutionService
{
    private readonly IConflictEventRepository _conflictEventRepository;
    private readonly IItemRepository _itemRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ConflictResolutionService> _logger;

    public ConflictResolutionService(
        IConflictEventRepository conflictEventRepository,
        IItemRepository itemRepository,
        INotificationService notificationService,
        ILogger<ConflictResolutionService> logger)
    {
        _conflictEventRepository = conflictEventRepository;
        _itemRepository = itemRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ConflictResolutionResult> HandleConcurrencyConflictAsync(
        Guid itemId,
        Guid winningUserId,
        Guid losingUserId,
        byte[]? versionAtConflict,
        string conflictDetails,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Concurrency conflict detected for item {ItemId}. Winner: {WinningUserId}, Loser: {LosingUserId}",
            itemId,
            winningUserId,
            losingUserId);

        // Record the conflict event
        await RecordConflictEventAsync(
            itemId,
            winningUserId,
            losingUserId,
            versionAtConflict,
            conflictDetails,
            cancellationToken);

        // Get current item state
        var currentItem = await _itemRepository.TryReloadAsync(itemId, cancellationToken);

        if (currentItem == null)
        {
            _logger.LogError("Item {ItemId} not found when handling conflict", itemId);
            return ConflictResolutionResult.Unresolved("Item not found");
        }

        // Send conflict notification to losing user
        var notification = Notification.CreateSystemNotification(
            losingUserId,
            NotificationType.ConflictDetected,
            "Edit Conflict Detected",
            $"Your changes to an item could not be saved because another user modified it first. Please review the current version and try again.",
            currentItem.CollectionId,
            itemId,
            NotificationPriority.High);

        await _notificationService.CreateAndSendNotificationAsync(notification, cancellationToken);

        _logger.LogInformation(
            "Conflict notification sent to user {LosingUserId} for item {ItemId}",
            losingUserId,
            itemId);

        return ConflictResolutionResult.Conflict(
            winningUserId,
            losingUserId,
            currentItem,
            "Conflict resolved using last-write-wins. Notification sent to losing user.");
    }

    public async Task RecordConflictEventAsync(
        Guid itemId,
        Guid winningUserId,
        Guid losingUserId,
        byte[]? versionAtConflict,
        string conflictDetails,
        CancellationToken cancellationToken = default)
    {
        var conflictEvent = ConflictEvent.Create(
            itemId,
            winningUserId,
            losingUserId,
            versionAtConflict,
            conflictDetails);

        await _conflictEventRepository.AddAsync(conflictEvent, cancellationToken);
        await _conflictEventRepository.SaveChangesAsync(cancellationToken);

        conflictEvent.MarkNotificationSent();
        await _conflictEventRepository.UpdateAsync(conflictEvent, cancellationToken);
        await _conflictEventRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Conflict event {ConflictEventId} recorded for item {ItemId}",
            conflictEvent.ConflictEventId,
            itemId);
    }

    public async Task<int> GetConflictCountAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        return await _conflictEventRepository.GetConflictCountAsync(since, cancellationToken);
    }
}
