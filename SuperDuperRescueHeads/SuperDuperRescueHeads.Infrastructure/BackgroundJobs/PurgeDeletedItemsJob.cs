using Microsoft.Extensions.Logging;
using SuperDuperRescueHeads.Domain.Items;

namespace SuperDuperRescueHeads.Infrastructure.BackgroundJobs;

public class PurgeDeletedItemsJob
{
    private readonly IItemRepository _repository;
    private readonly ILogger<PurgeDeletedItemsJob> _logger;

    public PurgeDeletedItemsJob(IItemRepository repository, ILogger<PurgeDeletedItemsJob> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting purge of deleted items older than 30 days");

        try
        {
            var purgedCount = await _repository.PurgeExpiredItemsAsync(batchSize: 1000, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully purged {Count} expired items", purgedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge expired deleted items");
            throw;
        }
    }
}
