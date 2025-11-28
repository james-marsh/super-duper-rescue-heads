namespace SuperDuperRescueHeads.Domain.Items;

public record ItemCreatedEvent(
    Guid ItemId,
    Guid CollectionId,
    string Name,
    DateTimeOffset CreatedAt
) : DomainEvent;

public record ItemUpdatedEvent(
    Guid ItemId,
    string? OldName,
    string? NewName,
    DateTimeOffset UpdatedAt
) : DomainEvent;

public record ItemDeletedEvent(
    Guid ItemId,
    Guid CollectionId,
    DateTimeOffset DeletedAt
) : DomainEvent;

public record ItemRestoredEvent(
    Guid ItemId,
    Guid CollectionId,
    DateTimeOffset RestoredAt
) : DomainEvent;

public record ItemPermanentlyDeletedEvent(
    Guid ItemId,
    DateTimeOffset PurgedAt
) : DomainEvent;

// Base domain event class (placeholder - should exist from Feature 001)
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
