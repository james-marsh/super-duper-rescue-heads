using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Domain.Collections;

/// <summary>
/// Collection aggregate root representing a user's collection
/// </summary>
public class Collection
{
    public Guid CollectionId { get; private set; }
    public Guid OwnerId { get; private set; } // User who owns this collection
    public CollectionName Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Soft Delete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    // Optimistic Concurrency Control
    public byte[] RowVersion { get; private set; } = null!;

    // Navigation properties
    public ICollection<Item> Items { get; private set; } = new List<Item>();

    // Domain events
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // EF Core constructor
    private Collection() { }

    private Collection(Guid collectionId, Guid ownerId, CollectionName name, string? description)
    {
        CollectionId = collectionId;
        OwnerId = ownerId;
        Name = name;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        IsDeleted = false;
    }

    public static Collection Create(Guid ownerId, CollectionName name, string? description = null)
    {
        var collectionId = Guid.NewGuid();
        var collection = new Collection(collectionId, ownerId, name, description);

        collection.AddDomainEvent(new CollectionCreatedEvent(collectionId, ownerId, name.Value, collection.CreatedAt));

        return collection;
    }

    public void UpdateName(CollectionName newName)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update a deleted collection");

        var oldName = Name.Value;
        Name = newName;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CollectionUpdatedEvent(CollectionId, oldName, newName.Value, UpdatedAt));
    }

    public void UpdateDescription(string? newDescription)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update a deleted collection");

        if (newDescription?.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(newDescription));

        Description = newDescription;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Collection is already deleted");

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CollectionDeletedEvent(CollectionId, OwnerId, DeletedAt.Value));
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Collection is not deleted");

        IsDeleted = false;
        DeletedAt = null;

        AddDomainEvent(new CollectionRestoredEvent(CollectionId, OwnerId, DateTimeOffset.UtcNow));
    }

    public bool IsOwnedBy(Guid userId) => OwnerId == userId;

    public byte[] GetCurrentVersion() => RowVersion;

    private void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
