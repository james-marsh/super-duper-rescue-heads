namespace SuperDuperRescueHeads.Domain.Sharing;

public record EffectivePermission
{
    public SharePermission Permission { get; init; }
    public List<PermissionSource> Sources { get; init; } = new();

    public static EffectivePermission Create(SharePermission permission, List<PermissionSource> sources)
    {
        return new EffectivePermission
        {
            Permission = permission,
            Sources = sources
        };
    }
}

public record PermissionSource
{
    public required string Type { get; init; } // "Individual" or "Group"
    public Guid? GroupId { get; init; }
    public string? GroupName { get; init; }
    public required SharePermission Permission { get; init; }
}
