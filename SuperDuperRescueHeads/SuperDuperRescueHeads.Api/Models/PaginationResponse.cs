namespace SuperDuperRescueHeads.Api.Models;

public record PaginationResponse
{
    public required int Total { get; init; }
    public required int Skip { get; init; }
    public required int Take { get; init; }
    public required bool HasMore { get; init; }
}
