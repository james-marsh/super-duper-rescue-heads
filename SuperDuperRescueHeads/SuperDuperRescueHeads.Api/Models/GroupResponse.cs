namespace SuperDuperRescueHeads.Api.Models;

public record GroupResponse
{
    public required Guid UserGroupId { get; init; }
    public required string GroupName { get; init; }
    public string? Description { get; init; }
    public required int MemberCount { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
