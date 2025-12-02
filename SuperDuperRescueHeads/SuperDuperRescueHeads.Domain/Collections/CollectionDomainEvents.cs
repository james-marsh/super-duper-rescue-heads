using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Domain.Collections;

public record CollectionCreatedEvent(
    Guid CollectionId,
    Guid OwnerId,
    string Name,
    DateTimeOffset CreatedAt) : DomainEvent;

public record CollectionUpdatedEvent(
    Guid CollectionId,
    string OldName,
    string NewName,
    DateTimeOffset UpdatedAt) : DomainEvent;

public record CollectionDeletedEvent(
    Guid CollectionId,
    Guid OwnerId,
    DateTimeOffset DeletedAt) : DomainEvent;

public record CollectionRestoredEvent(
    Guid CollectionId,
    Guid OwnerId,
    DateTimeOffset RestoredAt) : DomainEvent;
