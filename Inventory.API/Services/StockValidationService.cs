using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;

namespace Inventory.API.Services;

public class StockValidationService : InventoryServiceBase
{
    public StockValidationService(
        InventoryDbContext context,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitRepository unitRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseProductRepository warehouseProductRepository)
        : base(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository)
    {
    }

    public async Task<StockValidationResponse?> ValidateStockAsync(string companyCen, StockValidationRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, dto.WarehouseCen);
        if (warehouse == null)
            return null;

        var requirements = new List<StockRequirementDto>();

        foreach (var item in dto.Items)
        {
            var product = await ResolveProductAsync(business.Id, item.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var available = await GetCurrentStockAsync(warehouse.Id, product.Id);
            if (available < item.Quantity)
            {
                requirements.Add(new StockRequirementDto
                {
                    ProductCen = product.Cen ?? string.Empty,
                    ProductName = product.Name ?? string.Empty,
                    WarehouseCen = warehouse.Cen ?? string.Empty,
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = available,
                    MissingQuantity = item.Quantity - available,
                    UnitName = product.Unit?.Name ?? string.Empty,
                    Reason = "INSUFFICIENT_STOCK"
                });
            }
        }

        return new StockValidationResponse
        {
            IsValid = requirements.Count == 0,
            Requirements = requirements
        };
    }
}
