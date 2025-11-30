using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SuperDuperRescueHeads.Domain.Groups;

namespace SuperDuperRescueHeads.Api.Endpoints;

public static class GroupMembershipWebhookEndpoint
{
    public static IEndpointRouteBuilder MapGroupMembershipWebhooks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/webhooks")
            .WithTags("Webhooks");
        // Note: Webhooks should NOT require authorization as they come from external systems

        // POST /api/v1/webhooks/group-membership
        group.MapPost("/group-membership", async (
            HttpContext context,
            [FromBody] GroupMembershipWebhookPayload payload,
            IGroupSyncService groupSyncService,
            IConfiguration configuration,
            ILogger<GroupMembershipWebhookPayload> logger,
            CancellationToken cancellationToken) =>
        {
            // Validate webhook signature for security (T066)
            var signature = context.Request.Headers["X-Webhook-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature))
            {
                logger.LogWarning("Webhook request received without signature");
                return Results.Unauthorized();
            }

            var webhookSecret = configuration["Webhooks:GroupMembership:Secret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                logger.LogError("Webhook secret not configured");
                return Results.Problem("Webhook not configured");
            }

            // Validate signature
            var isValid = ValidateWebhookSignature(
                context.Request.Body,
                signature,
                webhookSecret);

            if (!isValid)
            {
                logger.LogWarning("Webhook signature validation failed");
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Received group membership webhook for group {UserGroupId}: {MembersAdded} added, {MembersRemoved} removed",
                payload.UserGroupId,
                payload.MembersAdded?.Count ?? 0,
                payload.MembersRemoved?.Count ?? 0);

            try
            {
                // Process member additions
                if (payload.MembersAdded != null && payload.MembersAdded.Any())
                {
                    foreach (var userId in payload.MembersAdded)
                    {
                        await groupSyncService.ProcessMemberAddedAsync(
                            payload.UserGroupId,
                            userId,
                            payload.GroupName ?? "Unknown Group",
                            cancellationToken);
                    }
                }

                // Process member removals
                if (payload.MembersRemoved != null && payload.MembersRemoved.Any())
                {
                    foreach (var userId in payload.MembersRemoved)
                    {
                        await groupSyncService.ProcessMemberRemovedAsync(
                            payload.UserGroupId,
                            userId,
                            payload.GroupName ?? "Unknown Group",
                            cancellationToken);
                    }
                }

                logger.LogInformation(
                    "Successfully processed group membership webhook for group {UserGroupId}",
                    payload.UserGroupId);

                return Results.Ok(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing group membership webhook for group {UserGroupId}",
                    payload.UserGroupId);
                return Results.Problem("Error processing webhook");
            }
        })
        .WithName("GroupMembershipWebhook")
        .WithOpenApi();

        return app;
    }

    /// <summary>
    /// Validates webhook signature using HMAC-SHA256.
    /// This prevents unauthorized webhook calls from malicious actors.
    /// </summary>
    private static bool ValidateWebhookSignature(
        Stream requestBody,
        string providedSignature,
        string secret)
    {
        try
        {
            // Read request body
            using var reader = new StreamReader(requestBody, leaveOpen: true);
            var body = reader.ReadToEnd();
            requestBody.Position = 0; // Reset stream position for later reads

            // Compute HMAC-SHA256 hash
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            // Compare signatures (constant-time comparison to prevent timing attacks)
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(providedSignature.ToLowerInvariant()));
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Payload received from external user management system when group membership changes.
/// External systems should POST to /api/v1/webhooks/group-membership with this structure.
/// </summary>
public record GroupMembershipWebhookPayload
{
    public required Guid UserGroupId { get; init; }
    public string? GroupName { get; init; }
    public List<Guid>? MembersAdded { get; init; }
    public List<Guid>? MembersRemoved { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
