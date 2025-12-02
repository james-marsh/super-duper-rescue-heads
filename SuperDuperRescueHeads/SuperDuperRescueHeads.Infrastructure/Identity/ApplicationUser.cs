using Microsoft.AspNetCore.Identity;

namespace SuperDuperRescueHeads.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user entity
/// Bridges Identity framework with our domain User aggregate
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation to domain User aggregate
    public Guid DomainUserId { get; set; }
}
