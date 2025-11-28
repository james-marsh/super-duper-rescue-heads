namespace SuperDuperRescueHeads.Infrastructure.Data.Entities;

public class UserSearchHistory
{
    public Guid UserId { get; set; }
    public required string SearchTerm { get; set; }
    public DateTimeOffset SearchedAt { get; set; }
}
