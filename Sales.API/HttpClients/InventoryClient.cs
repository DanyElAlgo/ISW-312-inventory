using System.Net.Http.Json;
using Shared.Contracts.DTOs;

namespace Sales.API.HttpClients;

public class InventoryClient
{
    private readonly HttpClient _http;

    public InventoryClient(HttpClient http)
    {
        _http = http;
    }

    // Fetches a lightweight product reference from Inventory.API.
    // Returns null when the product does not exist (404) or Inventory.API is unreachable.
    public async Task<ProductReferenceDto?> GetProductReferenceAsync(int productId)
    {
        try
        {
            var response = await _http.GetAsync($"api/products/{productId}/reference");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductReferenceDto>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot validate product at this time.");
        }
    }

    public async Task<BulkStockCheckResultDto?> ValidateBulkStockAsync(BulkStockCheckDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/stock/bulk-validate", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BulkStockCheckResultDto>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot validate stock at this time.");
        }
    }

    public async Task<BulkStockDeductResultDto?> DeductBulkStockAsync(BulkStockDeductDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/stock/bulk-deduct", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BulkStockDeductResultDto>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot deduct stock at this time.");
        }
    }

    public async Task<List<WarehouseProductDto>?> GetLowStockItemsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/warehouseproducts/stock/low");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WarehouseProductDto>>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
