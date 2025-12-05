namespace SuperDuperRescueHeads.Api.Configuration;

/// <summary>
/// Configuration options for collection limits
/// </summary>
public class CollectionLimitsOptions
{
    public const string SectionName = "CollectionLimits";

    /// <summary>
    /// Maximum number of collections allowed per user (default: 100)
    /// </summary>
    public int MaxCollectionsPerUser { get; set; } = 100;
}
