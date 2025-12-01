namespace SuperDuperRescueHeads.Domain.Shared;

/// <summary>
/// Exception thrown when a concurrency conflict is detected
/// </summary>
public class ConcurrencyException : Exception
{
    public Guid EntityId { get; }
    public string EntityType { get; }
    public byte[]? ExpectedVersion { get; }
    public byte[]? ActualVersion { get; }

    public ConcurrencyException(
        Guid entityId,
        string entityType,
        byte[]? expectedVersion = null,
        byte[]? actualVersion = null)
        : base($"Concurrency conflict detected for {entityType} with ID {entityId}")
    {
        EntityId = entityId;
        EntityType = entityType;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public ConcurrencyException(
        Guid entityId,
        string entityType,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        EntityId = entityId;
        EntityType = entityType;
    }
}
