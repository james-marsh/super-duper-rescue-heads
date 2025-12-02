namespace SuperDuperRescueHeads.Api.Services;

public interface ICurrentUserService
{
    Guid GetUserId();
    string GetEmail();
    string GetDisplayName();
    bool IsAuthenticated();
}
