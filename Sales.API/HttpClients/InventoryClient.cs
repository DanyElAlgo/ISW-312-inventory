using System.Net.Http.Json;
using Sales.API.DTOs;

namespace Sales.API.HttpClients;

public class InventoryClient
{
    private readonly HttpClient _http;

    public InventoryClient(HttpClient http)
    {
        _http = http;
    }
    
    public async Task<InventoryProductDto?> GetProductAsync(string companyCen, string productCen)
    {
        try
        {
            var response = await _http.GetAsync($"api/inventory/companies/{companyCen}/products/{productCen}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InventoryProductDto>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot fetch product at this time.");
        }
    }

    public async Task<List<InventoryStockItemDto>?> GetStockAsync(string companyCen, string? productCen, string? warehouseCen)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(productCen))
                query.Add($"productCen={Uri.EscapeDataString(productCen)}");
            if (!string.IsNullOrWhiteSpace(warehouseCen))
                query.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");

            var queryString = query.Count > 0 ? $"?{string.Join("&", query)}" : string.Empty;
            var response = await _http.GetAsync($"api/inventory/companies/{companyCen}/stock{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<InventoryStockItemDto>>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot read stock at this time.");
        }
    }

    public async Task<List<InventorySellableProductDto>?> GetSellableProductsAsync(
        string companyCen,
        string? search,
        string? categoryCen,
        string? warehouseCen,
        bool onlyAvailable,
        int page,
        int pageSize)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(categoryCen))
                query.Add($"categoryCen={Uri.EscapeDataString(categoryCen)}");
            if (!string.IsNullOrWhiteSpace(warehouseCen))
                query.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");
            query.Add($"onlyAvailable={onlyAvailable.ToString().ToLower()}");
            query.Add($"page={page}");
            query.Add($"pageSize={pageSize}");

            var queryString = $"?{string.Join("&", query)}";
            var response = await _http.GetAsync($"api/inventory/companies/{companyCen}/sellable-products{queryString}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<InventorySellableProductDto>>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot fetch product catalog at this time.");
        }
    }

    public async Task<StockValidationResponse?> ValidateStockAsync(string companyCen, StockValidationRequest dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/inventory/companies/{companyCen}/stock/validate", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StockValidationResponse>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot validate stock at this time.");
        }
    }

    public async Task<StockConsumeResponse?> ConsumeStockAsync(string companyCen, StockConsumeRequest dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/inventory/companies/{companyCen}/stock/consume", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StockConsumeResponse>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot deduct stock at this time.");
        }
    }
}
