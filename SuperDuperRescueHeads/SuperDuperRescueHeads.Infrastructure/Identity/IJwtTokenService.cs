using System.Security.Claims;

namespace SuperDuperRescueHeads.Infrastructure.Identity;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
}
