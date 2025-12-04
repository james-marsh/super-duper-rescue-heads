using System.Text.Json;
using SuperDuperRescueHeads.Domain.Collections;

namespace SuperDuperRescueHeads.Domain.Items;

public class Item
{
    public Guid ItemId { get; private set; }
    public Guid CollectionId { get; private set; }
    public ItemName Name { get; private set; } = null!;
    public string? Notes { get; private set; }
    public Dictionary<string, object> Attributes { get; private set; } = new();
    public DateTimeOffset? AcquisitionDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Soft Delete (Feature 003)
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    // Optimistic Concurrency Control (Feature 009)
    public byte[] RowVersion { get; private set; } = null!;

    // Navigation properties (EF Core)
    public Collection Collection { get; private set; } = null!;

    // Domain events
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // EF Core constructor
    private Item() { }

    private Item(Guid itemId, Guid collectionId, ItemName name,
        Dictionary<string, object> attributes, string? notes, DateTimeOffset? acquisitionDate)
    {
        ItemId = itemId;
        CollectionId = collectionId;
        Name = name;
        Attributes = attributes;
        Notes = notes;
        AcquisitionDate = acquisitionDate;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;

        ValidateAttributes();
    }

    public static Item Create(Guid collectionId, ItemName name,
        Dictionary<string, object> attributes, string? notes = null, DateTimeOffset? acquisitionDate = null)
    {
        var itemId = Guid.NewGuid();
        var item = new Item(itemId, collectionId, name, attributes, notes, acquisitionDate);

        item.AddDomainEvent(new ItemCreatedEvent(itemId, collectionId, name.Value, item.CreatedAt));

        return item;
    }

    public void UpdateName(ItemName newName)
    {
        var oldName = Name.Value;
        Name = newName;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ItemUpdatedEvent(ItemId, oldName, newName.Value, UpdatedAt));
    }

    public void UpdateNotes(string? newNotes)
    {
        if (newNotes?.Length > 1000)
            throw new ArgumentException("Notes cannot exceed 1000 characters", nameof(newNotes));

        Notes = newNotes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAttributes(Dictionary<string, object> newAttributes)
    {
        Attributes = newAttributes;
        UpdatedAt = DateTimeOffset.UtcNow;
        ValidateAttributes();
    }

    public void UpdateAcquisitionDate(DateTimeOffset? newDate)
    {
        AcquisitionDate = newDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool BelongsToCollection(Guid collectionId) => CollectionId == collectionId;

    public bool BelongsToUser(Guid userId)
    {
        // Check via Collection.OwnerId navigation property
        return Collection?.OwnerId == userId;
    }

    // Feature 009: Get current version for concurrency control
    public byte[] GetCurrentVersion() => RowVersion;

    // Soft Delete Methods (Feature 003)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Item is already deleted");

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DeletedAt.Value;

        AddDomainEvent(new ItemDeletedEvent(ItemId, CollectionId, DeletedAt.Value));
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Item is not deleted");

        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ItemRestoredEvent(ItemId, CollectionId, UpdatedAt));
    }

    private void ValidateAttributes()
    {
        var json = JsonSerializer.Serialize(Attributes);
        if (json.Length > 10 * 1024) // 10KB
            throw new ArgumentException("Attributes cannot exceed 10KB");
    }

    private void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
