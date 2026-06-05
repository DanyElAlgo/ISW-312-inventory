using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;

namespace Inventory.API.Services;

public class WarehousesService : InventoryServiceBase
{
    public WarehousesService(
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

    public async Task<IReadOnlyList<WarehouseDto>?> GetWarehousesAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouses = await WarehouseRepository.GetByBusinessIdAsync(business.Id);

        return warehouses.Select(MapWarehouse).ToList();
    }

    public async Task<WarehouseDto?> CreateWarehouseAsync(string companyCen, CreateWarehouseRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Warehouse name is required.");

        var warehouse = new Warehouse
        {
            BusinessId = business.Id,
            Name = dto.Name.Trim(),
            IsActive = true
        };

        await WarehouseRepository.AddAsync(warehouse);
        await Context.SaveChangesAsync();

        warehouse.Cen = BuildCen("WAR", warehouse.Id);
        await Context.SaveChangesAsync();

        return MapWarehouse(warehouse);
    }

    public async Task<WarehouseDto?> UpdateWarehouseAsync(string companyCen, string warehouseCen, UpdateWarehouseRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, warehouseCen);
        if (warehouse == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            warehouse.Name = dto.Name.Trim();

        if (dto.IsActive.HasValue)
            warehouse.IsActive = dto.IsActive.Value;

        await Context.SaveChangesAsync();
        return MapWarehouse(warehouse);
    }
}
