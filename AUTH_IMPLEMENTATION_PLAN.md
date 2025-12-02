# ASP.NET Core Identity Implementation Plan
**Feature**: User Authentication & Authorization
**Strategy**: ASP.NET Core Identity + JWT Bearer Tokens
**Date**: 2025-12-01
**Branch**: feature/authentication-identity

---

## Executive Summary

Implement comprehensive authentication and authorization using ASP.NET Core Identity with JWT bearer tokens to replace all 39 `Guid.Empty` placeholders throughout the solution and enable proper user-specific data isolation.

---

## Architecture Overview

### Technology Stack
- **ASP.NET Core Identity**: User management, password hashing, role management
- **JWT Bearer Tokens**: Stateless authentication for API endpoints
- **Entity Framework Core**: Identity persistence with existing ApplicationDbContext
- **Domain-Driven Design**: User aggregate in Domain layer, Identity in Infrastructure

### Authentication Flow
1. User registers → Identity creates user → Returns JWT token
2. User logs in → Identity validates credentials → Returns JWT token
3. Client includes JWT in Authorization header
4. API validates JWT → Extracts user claims → Populates HttpContext.User
5. Endpoints access current user via ClaimsPrincipal

---

## Domain Model

### User Aggregate (Domain Layer)

**Location**: `SuperDuperRescueHeads.Domain/Users/User.cs`

```csharp
namespace SuperDuperRescueHeads.Domain.Users;

/// <summary>
/// User aggregate root representing an application user
/// </summary>
public class User
{
    public Guid UserId { get; private set; }
    public Email Email { get; private set; } // Value object
    public DisplayName DisplayName { get; private set; } // Value object
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Collections
    public ICollection<Collection> OwnedCollections { get; private set; } = new List<Collection>();

    private User() { } // EF Core

    public static User Create(Email email, DisplayName displayName)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateDisplayName(DisplayName newDisplayName)
    {
        DisplayName = newDisplayName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
```

### Value Objects

**Email.cs**:
```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");

        return new Email(email.ToLowerInvariant().Trim());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**DisplayName.cs**:
```csharp
public sealed class DisplayName : ValueObject
{
    public string Value { get; }

    private DisplayName(string value)
    {
        Value = value;
    }

    public static DisplayName Create(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty");

        if (displayName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters");

        return new DisplayName(displayName.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

---

## Infrastructure Layer

### ApplicationUser (Identity Integration)

**Location**: `SuperDuperRescueHeads.Infrastructure/Identity/ApplicationUser.cs`

```csharp
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
```

### JWT Token Service

**Location**: `SuperDuperRescueHeads.Infrastructure/Identity/JwtTokenService.cs`

```csharp
public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.DisplayName),
            new("domain_user_id", user.DomainUserId.ToString())
        };

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### User Repository

**Location**: `SuperDuperRescueHeads.Infrastructure/Repositories/UserRepository.cs`

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default);
}
```

---

## API Layer

### Authentication Endpoints

**Location**: `SuperDuperRescueHeads.Api/Endpoints/AuthenticationEndpoints.cs`

```csharp
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication")
            .WithOpenApi();

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
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        CancellationToken ct)
    {
        // Validate request
        var email = Email.Create(request.Email);
        var displayName = DisplayName.Create(request.DisplayName);

        // Create domain user
        var domainUser = User.Create(email, displayName);
        await userRepository.AddAsync(domainUser, ct);

        // Create identity user
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

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        CancellationToken ct)
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
}
```

### Current User Service

**Location**: `SuperDuperRescueHeads.Api/Services/CurrentUserService.cs`

```csharp
public interface ICurrentUserService
{
    Guid GetUserId();
    string GetEmail();
    string GetDisplayName();
    bool IsAuthenticated();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("domain_user_id");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedException("User", "access");
        }

        return userId;
    }

    public string GetEmail()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value
            ?? throw new UnauthorizedException("User", "access");
    }

    public string GetDisplayName()
    {
        return _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Name)?.Value
            ?? "Unknown";
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated
            ?? false;
    }
}
```

---

## Configuration

### Program.cs Updates

```csharp
// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR JWT support
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Add authorization
builder.Services.AddAuthorization();

// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// In app configuration
app.UseAuthentication();
app.UseAuthorization();
```

### appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-here-change-in-production",
    "Issuer": "SuperDuperRescueHeads.Api",
    "Audience": "SuperDuperRescueHeads.Web",
    "ExpirationHours": 8
  }
}
```

---

## Database Migration

### Migration for Identity Tables

```bash
dotnet ef migrations add AddIdentityTables -p SuperDuperRescueHeads.Infrastructure -s SuperDuperRescueHeads.Api
```

**Expected Tables**:
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- AspNetRoleClaims

### Migration for User Domain Entity

```bash
dotnet ef migrations add AddUserAggregate -p SuperDuperRescueHeads.Infrastructure -s SuperDuperRescueHeads.Api
```

**Expected Changes**:
- Add Users table (domain entity)
- Add foreign key from Items to Users
- Add foreign key from Collections to Users
- Add indexes on UserId columns

---

## Endpoint Updates (39 Locations)

### Pattern for Updating Endpoints

**Before**:
```csharp
public static async Task<IResult> GetUnreadNotifications(
    INotificationService notificationService,
    CancellationToken cancellationToken)
{
    var currentUserId = Guid.Empty; // TODO: Get from claims
    var notifications = await notificationService.GetUnreadNotificationsAsync(
        currentUserId,
        cancellationToken);
    return Results.Ok(notifications);
}
```

**After**:
```csharp
public static async Task<IResult> GetUnreadNotifications(
    ICurrentUserService currentUserService,
    INotificationService notificationService,
    CancellationToken cancellationToken)
{
    var currentUserId = currentUserService.GetUserId();
    var notifications = await notificationService.GetUnreadNotificationsAsync(
        currentUserId,
        cancellationToken);
    return Results.Ok(notifications);
}
```

### Files to Update (39 TODOs)

1. **CollectionSharingEndpoints.cs** (15 TODOs)
   - ShareCollectionAsync
   - GetSharedWithMeAsync
   - GetCollectionSharesAsync
   - UpdateSharePermissionsAsync
   - RevokeShareAsync

2. **NotificationEndpoints.cs** (5 TODOs)
   - GetUnreadNotifications
   - GetNotifications
   - MarkAsRead
   - MarkAllAsRead
   - DismissNotification

3. **DeletedItemsEndpoints.cs** (3 TODOs)
   - GetDeletedItems
   - RestoreItem
   - PermanentlyDeleteItem

4. **GroupSharingEndpoints.cs** (7 TODOs)
   - ShareWithGroup
   - GetGroupShares
   - RemoveGroupShare
   - GetMyGroups
   - AddGroupMembers
   - RemoveGroupMembers

5. **SearchEndpoints.cs** (3 TODOs)
   - Search
   - GetSuggestions
   - GetRecentSearches

6. **NotificationHub.cs** (2 TODOs)
   - OnConnectedAsync
   - OnDisconnectedAsync

7. **CollectionPermissionHandler.cs** (1 TODO)
   - HandleRequirementAsync

8. **ItemsEndpoints.cs** (1 TODO + 5 implementations needed)
   - All CRUD operations

---

## Testing Strategy

### Unit Tests

**Location**: `SuperDuperRescueHeads.Tests.Unit/Domain/Users/`

- UserTests.cs - Domain entity behavior
- EmailTests.cs - Email value object validation
- DisplayNameTests.cs - DisplayName value object validation

### Integration Tests

**Location**: `SuperDuperRescueHeads.Tests.Integration/Infrastructure/Identity/`

- UserRepositoryTests.cs - User persistence
- JwtTokenServiceTests.cs - Token generation/validation

### Contract Tests

**Location**: `SuperDuperRescueHeads.Tests.Contract/Endpoints/`

- AuthenticationEndpointsTests.cs - Registration, login, token refresh
- Test invalid credentials
- Test duplicate email registration
- Test password validation rules

---

## Implementation Tasks (Ordered)

### Phase 1: Foundation (Days 1-2)

- [ ] **Task 1.1**: Create User aggregate and value objects
  - User.cs
  - Email.cs
  - DisplayName.cs

- [ ] **Task 1.2**: Create ApplicationUser and Identity infrastructure
  - ApplicationUser.cs
  - UserRepository.cs
  - UserConfiguration.cs (EF Core)

- [ ] **Task 1.3**: Create database migrations
  - Identity tables migration
  - User domain entity migration
  - Update existing entities with UserId foreign keys

### Phase 2: Authentication Services (Days 3-4)

- [ ] **Task 2.1**: Implement JWT token service
  - IJwtTokenService interface
  - JwtTokenService implementation
  - Configuration in appsettings.json

- [ ] **Task 2.2**: Implement CurrentUserService
  - ICurrentUserService interface
  - CurrentUserService implementation
  - HttpContextAccessor integration

- [ ] **Task 2.3**: Configure authentication in Program.cs
  - Add Identity services
  - Add JWT bearer authentication
  - Add authorization services
  - Configure middleware pipeline

### Phase 3: Authentication Endpoints (Days 5-6)

- [ ] **Task 3.1**: Create authentication DTOs
  - RegisterRequest
  - LoginRequest
  - AuthenticationResponse
  - UserResponse

- [ ] **Task 3.2**: Implement AuthenticationEndpoints
  - POST /api/v1/auth/register
  - POST /api/v1/auth/login
  - POST /api/v1/auth/refresh
  - GET /api/v1/auth/me

- [ ] **Task 3.3**: Add input validation
  - Email format validation
  - Password strength validation
  - Display name validation

### Phase 4: Update Existing Endpoints (Days 7-10)

- [ ] **Task 4.1**: Update CollectionSharingEndpoints (15 TODOs)
- [ ] **Task 4.2**: Update NotificationEndpoints (5 TODOs)
- [ ] **Task 4.3**: Update GroupSharingEndpoints (7 TODOs)
- [ ] **Task 4.4**: Update SearchEndpoints (3 TODOs)
- [ ] **Task 4.5**: Update DeletedItemsEndpoints (3 TODOs)
- [ ] **Task 4.6**: Update ItemsEndpoints (1 TODO + implement 5 stubs)
- [ ] **Task 4.7**: Update NotificationHub (2 TODOs)
- [ ] **Task 4.8**: Update CollectionPermissionHandler (1 TODO)

### Phase 5: Testing (Days 11-12)

- [ ] **Task 5.1**: Write unit tests
  - User entity tests
  - Value object tests
  - JWT service tests

- [ ] **Task 5.2**: Write integration tests
  - Authentication flow tests
  - User repository tests
  - Token validation tests

- [ ] **Task 5.3**: Write contract tests
  - Registration endpoint tests
  - Login endpoint tests
  - Protected endpoint tests

### Phase 6: Documentation & Polish (Days 13-14)

- [ ] **Task 6.1**: Add XML documentation to all endpoints
- [ ] **Task 6.2**: Update OpenAPI/Swagger descriptions
- [ ] **Task 6.3**: Create authentication documentation
- [ ] **Task 6.4**: Add example requests/responses
- [ ] **Task 6.5**: Security review and audit
- [ ] **Task 6.6**: Move JWT secret to environment variables/Key Vault

---

## Security Considerations

### Password Security
- ✅ ASP.NET Core Identity handles password hashing (PBKDF2)
- ✅ Configurable password strength requirements
- ✅ Account lockout after failed attempts
- ✅ Password reset tokens

### Token Security
- ✅ JWT signed with HMAC-SHA256
- ✅ Short expiration time (8 hours)
- ✅ Token validation on every request
- ⚠️ Consider refresh token rotation for production
- ⚠️ Store JWT secret in Azure Key Vault for production

### Authorization
- ✅ RequireAuthorization() on all protected endpoints
- ✅ User-specific data filtering via CurrentUserService
- ✅ Owner validation in CollectionPermissionHandler
- ⚠️ Add role-based authorization if needed
- ⚠️ Add policy-based authorization for complex scenarios

---

## Success Criteria

- [ ] All 39 `Guid.Empty` TODOs resolved
- [ ] User registration and login working
- [ ] JWT tokens generated and validated correctly
- [ ] Protected endpoints require valid JWT
- [ ] CurrentUserService extracts user from claims
- [ ] All existing endpoints use real user IDs
- [ ] SignalR hubs authenticate via JWT
- [ ] Database migrations applied successfully
- [ ] Minimum 80% test coverage for auth code
- [ ] Security audit completed
- [ ] Documentation complete

---

## Risks & Mitigations

### Risk 1: Breaking Existing Functionality
**Impact**: High
**Mitigation**:
- Comprehensive testing before merging
- Feature flag for authentication (optional)
- Rollback plan via git revert

### Risk 2: Performance Impact
**Impact**: Medium
**Mitigation**:
- JWT validation is fast (< 1ms)
- Cache user claims in token
- Use distributed cache for user lookups if needed

### Risk 3: Database Migration Issues
**Impact**: High
**Mitigation**:
- Test migrations on dev database first
- Create rollback migration scripts
- Backup database before applying migrations

### Risk 4: Security Vulnerabilities
**Impact**: Critical
**Mitigation**:
- Follow OWASP guidelines
- Use ASP.NET Core Identity defaults (proven secure)
- Security code review
- Penetration testing

---

## Future Enhancements (Post-MVP)

1. **OAuth2 Integration**
   - Google authentication
   - Microsoft authentication
   - GitHub authentication

2. **Two-Factor Authentication**
   - TOTP (Google Authenticator)
   - SMS verification
   - Email verification

3. **Advanced Authorization**
   - Role-based access control (RBAC)
   - Attribute-based access control (ABAC)
   - Custom authorization policies

4. **Token Management**
   - Refresh token rotation
   - Token revocation
   - Session management

5. **Audit Logging**
   - Track login attempts
   - Track failed authentications
   - Track privilege escalation

---

## References

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Microsoft Identity Security Best Practices](https://learn.microsoft.com/en-us/security/benchmark/azure/baselines/aad-security-baseline)
