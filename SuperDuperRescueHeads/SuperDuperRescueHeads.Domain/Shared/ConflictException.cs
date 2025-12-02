namespace SuperDuperRescueHeads.Domain.Shared;

/// <summary>
/// Exception thrown when a request conflicts with the current state of a resource
/// </summary>
public class ConflictException : Exception
{
    public string? Resource { get; }
    public string? ConflictReason { get; }

    public ConflictException()
        : base("The request conflicts with the current state of the resource")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string resource, string conflictReason)
        : base($"Conflict with {resource}: {conflictReason}")
    {
        Resource = resource;
        ConflictReason = conflictReason;
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
