using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;

namespace Inventory.API.Services;

public class CompaniesService : InventoryServiceBase
{
    public CompaniesService(
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

    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync()
    {
        var businesses = await BusinessRepository.GetAllAsync();

        return businesses.Select(b => new CompanyDto
        {
            CompanyCen = b.Cen ?? BuildCen("COMP", b.Id),
            Name = b.Name ?? string.Empty,
            IsActive = b.IsActive
        }).ToList();
    }

    public async Task<CompanyLookupContractDto?> GetCompanyAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        return new CompanyLookupContractDto
        {
            CompanyId = business.Id,
            CompanyCen = business.Cen ?? BuildCen("COMP", business.Id),
            Name = business.Name ?? string.Empty
        };
    }

    public async Task<InventoryDashboardDto?> GetDashboardAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var productStocks = await ProductRepository.GetByBusinessIdAsync(business.Id);

        var totalProducts = productStocks.Count;
        var totalStockQuantity = productStocks.Sum(p => p.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0));
        var lowStockCount = productStocks.Count(p => p.ReorderLevel > 0 && p.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0) > 0 && p.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0) <= p.ReorderLevel);
        var outOfStockCount = productStocks.Count(p => p.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0) <= 0);

        return new InventoryDashboardDto
        {
            CompanyCen = business.Cen ?? BuildCen("COMP", business.Id),
            TotalProducts = totalProducts,
            TotalStockQuantity = totalStockQuantity,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount
        };
    }
}
