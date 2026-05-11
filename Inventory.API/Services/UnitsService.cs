using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;

namespace Inventory.API.Services;

public class UnitsService : InventoryServiceBase
{
    public UnitsService(
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

    public async Task<IReadOnlyList<UnitDto>?> GetUnitsAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var units = await UnitRepository.GetByBusinessIdAsync(business.Id);

        return units.Select(MapUnit).ToList();
    }

    public async Task<UnitDto?> CreateUnitAsync(string companyCen, CreateUnitRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Unit name is required.");

        var unit = new Unit
        {
            BusinessId = business.Id,
            Name = dto.Name.Trim(),
            Abbreviation = dto.Abbreviation,
            IsActive = true
        };

        await UnitRepository.AddAsync(unit);
        await Context.SaveChangesAsync();

        unit.Cen = BuildCen("UNIT", unit.Id);
        await Context.SaveChangesAsync();

        return MapUnit(unit);
    }

    public async Task<UnitDto?> UpdateUnitAsync(string companyCen, string unitCen, UpdateUnitRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var unit = await ResolveUnitAsync(business.Id, unitCen);
        if (unit == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            unit.Name = dto.Name.Trim();

        if (dto.Abbreviation != null)
            unit.Abbreviation = dto.Abbreviation;

        if (dto.IsActive.HasValue)
            unit.IsActive = dto.IsActive.Value;

        await Context.SaveChangesAsync();
        return MapUnit(unit);
    }
}
