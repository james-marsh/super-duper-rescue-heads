namespace SuperDuperRescueHeads.Domain.Sharing;

public class CollectionShare
{
    public Guid CollectionShareId { get; private set; }
    public Guid CollectionId { get; private set; }
    public Guid SharedWithUserId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public SharePermission Permission { get; private set; }
    public ShareStatus Status { get; private set; }
    public string InvitationToken { get; private set; }
    public string Email { get; private set; }
    public DateTimeOffset InvitedAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? LastAccessedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CollectionShare()
    {
        // EF Core constructor
        InvitationToken = null!;
        Email = null!;
    }

    public static CollectionShare CreateInvitation(
        Guid collectionId,
        Guid invitedByUserId,
        string email,
        Guid sharedWithUserId,
        SharePermission permission)
    {
        if (collectionId == Guid.Empty)
            throw new ArgumentException("Collection ID cannot be empty", nameof(collectionId));

        if (invitedByUserId == Guid.Empty)
            throw new ArgumentException("Invited by user ID cannot be empty", nameof(invitedByUserId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (invitedByUserId == sharedWithUserId)
            throw new InvalidOperationException("Cannot share collection with yourself");

        var now = DateTimeOffset.UtcNow;
        var token = GenerateSecureToken();

        return new CollectionShare
        {
            CollectionShareId = Guid.NewGuid(),
            CollectionId = collectionId,
            InvitedByUserId = invitedByUserId,
            SharedWithUserId = sharedWithUserId,
            Email = email,
            Permission = permission,
            Status = ShareStatus.Pending,
            InvitationToken = token,
            InvitedAt = now,
            ExpiresAt = now.AddDays(7),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Accept()
    {
        if (Status != ShareStatus.Pending)
            throw new InvalidOperationException($"Cannot accept invitation with status {Status}");

        if (DateTimeOffset.UtcNow > ExpiresAt)
        {
            Status = ShareStatus.Expired;
            throw new InvalidOperationException("Invitation has expired");
        }

        Status = ShareStatus.Accepted;
        AcceptedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Decline()
    {
        if (Status != ShareStatus.Pending)
            throw new InvalidOperationException($"Cannot decline invitation with status {Status}");

        Status = ShareStatus.Declined;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        if (Status == ShareStatus.Revoked)
            throw new InvalidOperationException("Invitation is already revoked");

        Status = ShareStatus.Revoked;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePermission(SharePermission newPermission)
    {
        if (Status != ShareStatus.Accepted)
            throw new InvalidOperationException("Cannot change permission for non-accepted invitation");

        Permission = newPermission;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTimeOffset.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        // Generate 128-bit (16 bytes) cryptographically secure random token
        var bytes = new byte[16];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
