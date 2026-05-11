using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Services;

public class KardexService : InventoryServiceBase
{
    public KardexService(
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

    public async Task<IReadOnlyList<KardexMovementDto>?> GetKardexAsync(
        string companyCen,
        string productCen,
        string? warehouseCen,
        DateTime? from,
        DateTime? to)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        int? warehouseId = null;
        if (!string.IsNullOrWhiteSpace(warehouseCen))
        {
            var warehouse = await ResolveWarehouseAsync(business.Id, warehouseCen);
            if (warehouse == null)
                return null;

            warehouseId = warehouse.Id;
        }

        var query = Context.Kardices
            .Include(k => k.Warehouse)
            .Include(k => k.Document)
            .Where(k => k.ProductId == product.Id)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(k => k.WarehouseId == warehouseId);

        if (from.HasValue)
            query = query.Where(k => k.TimeStamp >= from.Value);

        if (to.HasValue)
            query = query.Where(k => k.TimeStamp <= to.Value);

        var results = await query
            .OrderByDescending(k => k.TimeStamp)
            .ToListAsync();

        return results.Select(k => new KardexMovementDto
        {
            MovementCen = k.MovementCen ?? string.Empty,
            DocumentCen = k.Document?.DocumentCen,
            ProductCen = product.Cen ?? string.Empty,
            WarehouseCen = k.Warehouse?.Cen ?? string.Empty,
            MovementType = k.ActionType ?? string.Empty,
            Quantity = (decimal)(k.ActionQty ?? 0),
            UnitCost = null,
            Reason = k.Reason,
            CreatedAt = k.TimeStamp ?? DateTime.UtcNow
        }).ToList();
    }
}
