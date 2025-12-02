namespace SuperDuperRescueHeads.Domain.Shared;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }

    public NotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public NotFoundException(string entityType, object entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public NotFoundException(string entityType, object entityId, string message, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
