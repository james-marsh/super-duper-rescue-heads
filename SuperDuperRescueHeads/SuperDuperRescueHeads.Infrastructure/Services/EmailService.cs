using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Sharing;

namespace SuperDuperRescueHeads.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendInvitationEmailAsync(
        string recipientEmail,
        string inviterName,
        string collectionName,
        string invitationToken,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement SendGrid integration
        _logger.LogInformation(
            "Sending invitation email to {RecipientEmail}. Inviter: {InviterName}, Collection: {CollectionName}, Token: {InvitationToken}",
            recipientEmail, inviterName, collectionName, invitationToken);

        // For now, just log the email. In production, this would integrate with SendGrid
        // Example invitation link: https://app.example.com/invitations/accept?token={invitationToken}

        return Task.CompletedTask;
    }

    public Task SendInvitationAcceptedEmailAsync(
        string recipientEmail,
        string accepterName,
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending invitation accepted email to {RecipientEmail}. Accepter: {AccepterName}, Collection: {CollectionName}",
            recipientEmail, accepterName, collectionName);

        return Task.CompletedTask;
    }

    public Task SendInvitationRevokedEmailAsync(
        string recipientEmail,
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending invitation revoked email to {RecipientEmail}. Collection: {CollectionName}",
            recipientEmail, collectionName);

        return Task.CompletedTask;
    }

    public Task SendPermissionChangedEmailAsync(
        string recipientEmail,
        string collectionName,
        string newPermission,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending permission changed email to {RecipientEmail}. Collection: {CollectionName}, New Permission: {NewPermission}",
            recipientEmail, collectionName, newPermission);

        return Task.CompletedTask;
    }
}
