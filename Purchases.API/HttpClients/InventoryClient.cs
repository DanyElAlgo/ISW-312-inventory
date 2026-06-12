using System.Net;
using System.Net.Http.Json;
using Purchases.API.DTOs;
using Purchases.API.Exceptions;

namespace Purchases.API.HttpClients;

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

    public async Task<StockIncreaseResponse?> IncreaseStockAsync(string companyCen, StockIncreaseRequest dto)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/inventory/companies/{companyCen}/stock/increase", dto);

        await EnsureSuccessAsync(response, "increase stock");
        return await response.Content.ReadFromJsonAsync<StockIncreaseResponse>();
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
