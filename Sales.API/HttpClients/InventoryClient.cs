using System.Net;
using System.Net.Http.Json;
using Sales.API.DTOs;
using Sales.API.Exceptions;

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
        var response = await _http.GetAsync($"api/inventory/companies/{companyCen}/products/{productCen}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, "fetch product");
        return await response.Content.ReadFromJsonAsync<InventoryProductDto>();
    }

    public async Task<List<InventoryWarehouseDto>?> GetWarehousesAsync(string companyCen)
    {
        var response = await _http.GetAsync($"api/inventory/companies/{companyCen}/warehouses");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, "fetch warehouses");
        return await response.Content.ReadFromJsonAsync<List<InventoryWarehouseDto>>();
    }

    public async Task<List<InventoryStockItemDto>?> GetStockAsync(string companyCen, string? productCen, string? warehouseCen)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(productCen))
            query.Add($"productCen={Uri.EscapeDataString(productCen)}");
        if (!string.IsNullOrWhiteSpace(warehouseCen))
            query.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");

        var queryString = query.Count > 0 ? $"?{string.Join("&", query)}" : string.Empty;
        var response = await _http.GetAsync($"api/inventory/companies/{companyCen}/stock{queryString}");

        await EnsureSuccessAsync(response, "read stock");
        return await response.Content.ReadFromJsonAsync<List<InventoryStockItemDto>>();
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
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, "fetch product catalog");
        return await response.Content.ReadFromJsonAsync<List<InventorySellableProductDto>>();
    }

    public async Task<StockValidationResponse?> ValidateStockAsync(string companyCen, StockValidationRequest dto)
    {
        var response = await _http.PostAsJsonAsync($"api/inventory/companies/{companyCen}/stock/validate", dto);

        await EnsureSuccessAsync(response, "validate stock");
        return await response.Content.ReadFromJsonAsync<StockValidationResponse>();
    }

    public async Task<StockConsumeResponse?> ConsumeStockAsync(string companyCen, StockConsumeRequest dto)
    {
        var response = await _http.PostAsJsonAsync($"api/inventory/companies/{companyCen}/stock/consume", dto);

        await EnsureSuccessAsync(response, "deduct stock");
        return await response.Content.ReadFromJsonAsync<StockConsumeResponse>();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string action)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();

        if ((int)response.StatusCode >= 500)
            throw new InventoryUnavailableException(
                "El módulo de Inventario no está disponible en este momento. Intenta nuevamente en unos segundos.");

        throw new InvalidOperationException(
            $"Inventory rejected request to {action} ({(int)response.StatusCode}): {body}");
    }
}
