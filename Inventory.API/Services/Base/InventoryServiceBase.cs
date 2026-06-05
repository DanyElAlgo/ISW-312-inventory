using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;

namespace Inventory.API.Services.Base;

public abstract class InventoryServiceBase
{
    protected readonly InventoryDbContext Context;
    protected readonly IBusinessRepository BusinessRepository;
    protected readonly ICategoryRepository CategoryRepository;
    protected readonly IUnitRepository UnitRepository;
    protected readonly IWarehouseRepository WarehouseRepository;
    protected readonly IProductRepository ProductRepository;
    protected readonly IWarehouseProductRepository WarehouseProductRepository;

    protected const int ActiveStatusId = 1;
    protected const int OutOfStockStatusId = 4;

    protected InventoryServiceBase(
        InventoryDbContext context,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitRepository unitRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseProductRepository warehouseProductRepository)
    {
        Context = context;
        BusinessRepository = businessRepository;
        CategoryRepository = categoryRepository;
        UnitRepository = unitRepository;
        WarehouseRepository = warehouseRepository;
        ProductRepository = productRepository;
        WarehouseProductRepository = warehouseProductRepository;
    }

    protected async Task<Business?> ResolveBusinessAsync(string companyCen)
    {
        return await BusinessRepository.ResolveAsync(companyCen);
    }

    protected async Task<Category?> ResolveCategoryAsync(int businessId, string categoryCen)
    {
        return await CategoryRepository.ResolveAsync(businessId, categoryCen);
    }

    protected async Task<Unit?> ResolveUnitAsync(int businessId, string unitCen)
    {
        return await UnitRepository.ResolveAsync(businessId, unitCen);
    }

    protected async Task<Warehouse?> ResolveWarehouseAsync(int businessId, string warehouseCen)
    {
        return await WarehouseRepository.ResolveAsync(businessId, warehouseCen);
    }

    protected async Task<Product?> ResolveProductAsync(int businessId, string productCen)
    {
        return await ProductRepository.ResolveAsync(businessId, productCen);
    }

    protected async Task<decimal> GetCurrentStockAsync(int warehouseId, int productId)
    {
        return await WarehouseProductRepository.GetCurrentStockAsync(warehouseId, productId);
    }

    protected static string BuildCen(string prefix, int id)
    {
        return $"{prefix}-{id:D6}";
    }

    protected static string BuildDocumentCen(string prefix, int id)
    {
        return $"{prefix}-{id:D6}";
    }

    protected static string BuildMovementCen()
    {
        return $"MOV-{Guid.NewGuid():N}";
    }

    protected static CategoryDto MapCategory(Category category)
    {
        return new CategoryDto
        {
            CategoryCen = category.Cen ?? BuildCen("CAT", category.Id),
            Name = category.Name ?? string.Empty,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }

    protected static UnitDto MapUnit(Unit unit)
    {
        return new UnitDto
        {
            UnitCen = unit.Cen ?? BuildCen("UNIT", unit.Id),
            Name = unit.Name ?? string.Empty,
            Abbreviation = unit.Abbreviation,
            IsActive = unit.IsActive
        };
    }

    protected static WarehouseDto MapWarehouse(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            WarehouseCen = warehouse.Cen ?? BuildCen("WAR", warehouse.Id),
            Name = warehouse.Name ?? string.Empty,
            IsActive = warehouse.IsActive
        };
    }

    protected static ProductDto MapProduct(Product product, string status)
    {
        return new ProductDto
        {
            ProductCen = product.Cen ?? BuildCen("PRD", product.Id),
            Sku = product.Sku ?? product.Cen ?? BuildCen("PRD", product.Id),
            Name = product.Name ?? string.Empty,
            Description = product.Description,
            CategoryCen = product.Category?.Cen ?? string.Empty,
            CategoryName = product.Category?.Name ?? string.Empty,
            UnitCen = product.Unit?.Cen ?? string.Empty,
            UnitName = product.Unit?.Name ?? string.Empty,
            SalePrice = product.Price ?? 0,
            CostPrice = product.CostPrice,
            ReorderLevel = product.ReorderLevel,
            Status = status,
            StationCode = product.StationCode
        };
    }

    protected static string GetProductStatus(Product product)
    {
        if (!product.IsActive.GetValueOrDefault(true))
            return "INACTIVE";

        var totalStock = product.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0);
        if (totalStock <= 0)
            return "OUT_OF_STOCK";

        return "ACTIVE";
    }
}
