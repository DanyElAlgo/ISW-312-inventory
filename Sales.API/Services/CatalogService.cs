using Sales.API.DTOs;
using Sales.API.HttpClients;
using Microsoft.Extensions.Options;

namespace Sales.API.Services;

public class CatalogService
{
    private readonly InventoryClient _inventoryClient;

    public CatalogService(InventoryClient inventoryClient, IOptions<InventoryIntegrationOptions> _)
    {
        _inventoryClient = inventoryClient;
    }

    public async Task<IReadOnlyList<SellableProductContractDto>?> GetSellableProductsAsync(
        string companyCen,
        string? search,
        string? categoryCen,
        string? warehouseCen,
        bool onlyAvailable,
        int page,
        int pageSize)
    {
        var products = await _inventoryClient.GetSellableProductsAsync(
            companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize);

        if (products == null)
            return null;

        return products.Select(p => new SellableProductContractDto
        {
            ProductCen = p.ProductCen,
            Name = p.Name,
            CategoryCen = p.CategoryCen,
            CategoryName = p.CategoryName,
            SalePrice = p.SalePrice,
            AvailableQuantity = (double)p.AvailableQuantity,
            IsAvailable = p.IsAvailable,
            StationCode = p.StationCode
        }).ToList();
    }
}
