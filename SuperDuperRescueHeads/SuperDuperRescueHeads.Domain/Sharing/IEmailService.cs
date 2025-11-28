namespace SuperDuperRescueHeads.Domain.Sharing;

public interface IEmailService
{
    Task SendInvitationEmailAsync(
        string recipientEmail,
        string inviterName,
        string collectionName,
        string invitationToken,
        CancellationToken cancellationToken = default);

    Task SendInvitationAcceptedEmailAsync(
        string recipientEmail,
        string accepterName,
        string collectionName,
        CancellationToken cancellationToken = default);

    Task SendInvitationRevokedEmailAsync(
        string recipientEmail,
        string collectionName,
        CancellationToken cancellationToken = default);

    Task SendPermissionChangedEmailAsync(
        string recipientEmail,
        string collectionName,
        string newPermission,
        CancellationToken cancellationToken = default);
}
