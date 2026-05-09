using Microsoft.AspNetCore.Identity;
using SuperDuperRescueHeads.Api.Models;
using SuperDuperRescueHeads.Api.Services;
using SuperDuperRescueHeads.Domain.Shared;
using SuperDuperRescueHeads.Domain.Users;
using SuperDuperRescueHeads.Infrastructure.Data;
using SuperDuperRescueHeads.Infrastructure.Identity;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        // POST /api/v1/auth/register
        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .Produces<AuthenticationResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        // POST /api/v1/auth/login
        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate and receive JWT token")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // POST /api/v1/auth/refresh
        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh an expired JWT token")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK);

        // GET /api/v1/auth/me
        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user details")
            .RequireAuthorization()
            .Produces<UserResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Validate and create value objects
        var email = Email.Create(request.Email);
        var displayName = DisplayName.Create(request.DisplayName);

        // Use a database transaction to ensure atomicity of both user creations
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create domain user (not yet saved)
            var domainUser = User.Create(email, displayName);

            // Create identity user with reference to domain user
            var identityUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                DomainUserId = domainUser.UserId,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            // Create identity user first (performs validation: unique email, password strength, etc.)
            var result = await userManager.CreateAsync(identityUser, request.Password);

            if (!result.Succeeded)
            {
                throw new ValidationException(
                    result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    )
                );
            }

            // Create domain user - if this fails, the entire transaction will rollback
            await userRepository.AddAsync(domainUser, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            // Commit transaction - both users are now persisted atomically
            await transaction.CommitAsync(cancellationToken);

            var token = await jwtTokenService.GenerateTokenAsync(identityUser);

            return Results.Created($"/api/v1/auth/me", new AuthenticationResponse
            {
                Token = token,
                UserId = domainUser.UserId,
                Email = identityUser.Email!,
                DisplayName = identityUser.DisplayName,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
            });
        }
        catch
        {
            // Transaction will be rolled back automatically when disposed
            throw;
        }
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == null || !user.IsActive)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        var token = await jwtTokenService.GenerateTokenAsync(user);

        return Results.Ok(new AuthenticationResponse
        {
            Token = token,
            UserId = user.DomainUserId,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    private static async Task<IResult> RefreshTokenAsync(
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken)
    {
        // Get current user from token (even if expired, we can still read the claims)
        var email = currentUserService.GetEmail();
        var user = await userManager.FindByEmailAsync(email);

        if (user == null || !user.IsActive)
        {
            return Results.Unauthorized();
        }

        // Generate new token
        var token = await jwtTokenService.GenerateTokenAsync(user);

        return Results.Ok(new AuthenticationResponse
        {
            Token = token,
            UserId = user.DomainUserId,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var response = new UserResponse
        {
            UserId = user.UserId,
            Email = user.Email.Value,
            DisplayName = user.DisplayName.Value,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return Results.Ok(response);
    }
}
