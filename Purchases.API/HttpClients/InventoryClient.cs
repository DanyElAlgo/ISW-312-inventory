using System.Net.Http.Json;
using Purchases.API.DTOs;

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

    public async Task<StockIncreaseResponse?> IncreaseStockAsync(string companyCen, StockIncreaseRequest dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"api/inventory/companies/{companyCen}/stock/increase", dto);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Inventory rejected stock increase ({(int)response.StatusCode}): {body}");
            }

            return await response.Content.ReadFromJsonAsync<StockIncreaseResponse>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Inventory service is unavailable. Cannot increase stock at this time.");
        }
    }
}
