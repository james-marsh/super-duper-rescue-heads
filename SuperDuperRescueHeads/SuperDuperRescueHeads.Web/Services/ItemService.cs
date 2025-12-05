using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace SuperDuperRescueHeads.Web.Services;

public interface IItemService
{
    Task<List<ItemDto>> GetItemsAsync(Guid collectionId);
    Task<ItemDto?> GetItemAsync(Guid collectionId, Guid itemId);
    Task<ItemDto?> CreateItemAsync(Guid collectionId, CreateItemRequest request);
    Task<bool> UpdateItemAsync(Guid collectionId, Guid itemId, UpdateItemRequest request);
    Task<bool> DeleteItemAsync(Guid collectionId, Guid itemId);
}

public class ItemService : IItemService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ItemService> _logger;

    public ItemService(IHttpClientFactory httpClientFactory, ILogger<ItemService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<ItemDto>> GetItemsAsync(Guid collectionId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/v1/collections/{collectionId}/items");

            if (!response.IsSuccessStatusCode)
                return new List<ItemDto>();

            return await response.Content.ReadFromJsonAsync<List<ItemDto>>() ?? new List<ItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving items for collection {CollectionId}", collectionId);
            return new List<ItemDto>();
        }
    }

    public async Task<ItemDto?> GetItemAsync(Guid collectionId, Guid itemId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/v1/collections/{collectionId}/items/{itemId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving item {ItemId}", itemId);
            return null;
        }
    }

    public async Task<ItemDto?> CreateItemAsync(Guid collectionId, CreateItemRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync($"/api/v1/collections/{collectionId}/items", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating item {ItemName} in collection {CollectionId}", request.Name, collectionId);
            return null;
        }
    }

    public async Task<bool> UpdateItemAsync(Guid collectionId, Guid itemId, UpdateItemRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PutAsJsonAsync($"/api/v1/collections/{collectionId}/items/{itemId}", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating item {ItemId}", itemId);
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(Guid collectionId, Guid itemId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.DeleteAsync($"/api/v1/collections/{collectionId}/items/{itemId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting item {ItemId}", itemId);
            return false;
        }
    }
}

// DTOs
public record ItemDto(
    Guid ItemId,
    Guid CollectionId,
    string Name,
    string? Notes,
    Dictionary<string, object> Attributes,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateItemRequest(
    string Name,
    string? Notes,
    Dictionary<string, object> Attributes,
    DateTimeOffset? AcquisitionDate
);

public record UpdateItemRequest(
    string Name,
    string? Notes,
    Dictionary<string, object>? Attributes,
    DateTimeOffset? AcquisitionDate
);
