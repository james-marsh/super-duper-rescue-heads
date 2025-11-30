namespace SuperDuperRescueHeads.Api.Models;

/// <summary>
/// Represents a source of access to a collection (individual or group-based).
/// Used to show users how they're getting access to a collection.
/// </summary>
public record AccessSourceResponse
{
    /// <summary>
    /// Type of access source: "Individual" or "Group"
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Group ID if access is via group membership (null for individual access)
    /// </summary>
    public Guid? GroupId { get; init; }

    /// <summary>
    /// Group name if access is via group membership (null for individual access)
    /// </summary>
    public string? GroupName { get; init; }

    /// <summary>
    /// Permission level granted by this access source
    /// </summary>
    public required string Permission { get; init; }

    /// <summary>
    /// When this access was granted
    /// </summary>
    public required DateTimeOffset GrantedAt { get; init; }
}
