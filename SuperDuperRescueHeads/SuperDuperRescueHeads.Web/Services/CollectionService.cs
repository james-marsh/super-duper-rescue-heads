using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace SuperDuperRescueHeads.Web.Services;

public interface ICollectionService
{
    Task<List<CollectionDto>> GetCollectionsAsync();
    Task<CollectionDto?> GetCollectionAsync(Guid collectionId);
    Task<CollectionDto?> CreateCollectionAsync(CreateCollectionRequest request);
    Task<bool> UpdateCollectionAsync(Guid collectionId, UpdateCollectionRequest request);
    Task<bool> DeleteCollectionAsync(Guid collectionId);
}

public class CollectionService : ICollectionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(IHttpClientFactory httpClientFactory, ILogger<CollectionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<CollectionDto>> GetCollectionsAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/v1/collections");

            if (!response.IsSuccessStatusCode)
                return new List<CollectionDto>();

            return await response.Content.ReadFromJsonAsync<List<CollectionDto>>() ?? new List<CollectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving collections list");
            return new List<CollectionDto>();
        }
    }

    public async Task<CollectionDto?> GetCollectionAsync(Guid collectionId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/v1/collections/{collectionId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CollectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving collection {CollectionId}", collectionId);
            return null;
        }
    }

    public async Task<CollectionDto?> CreateCollectionAsync(CreateCollectionRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("/api/v1/collections", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CollectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating collection {CollectionName}", request.Name);
            return null;
        }
    }

    public async Task<bool> UpdateCollectionAsync(Guid collectionId, UpdateCollectionRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PutAsJsonAsync($"/api/v1/collections/{collectionId}", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating collection {CollectionId}", collectionId);
            return false;
        }
    }

    public async Task<bool> DeleteCollectionAsync(Guid collectionId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.DeleteAsync($"/api/v1/collections/{collectionId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting collection {CollectionId}", collectionId);
            return false;
        }
    }
}

// DTOs
public record CollectionDto(
    Guid CollectionId,
    string Name,
    string ItemType,
    string? Description,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ItemCount
);

public record CreateCollectionRequest(string Name, string ItemType, string? Description);
public record UpdateCollectionRequest(string Name, string? Description);
