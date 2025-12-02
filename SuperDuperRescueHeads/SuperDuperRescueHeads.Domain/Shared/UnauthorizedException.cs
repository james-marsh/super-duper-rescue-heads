namespace SuperDuperRescueHeads.Domain.Shared;

/// <summary>
/// Exception thrown when a user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : Exception
{
    public string? Resource { get; }
    public string? Action { get; }

    public UnauthorizedException()
        : base("You are not authorized to perform this action")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException(string resource, string action)
        : base($"You are not authorized to {action} {resource}")
    {
        Resource = resource;
        Action = action;
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
