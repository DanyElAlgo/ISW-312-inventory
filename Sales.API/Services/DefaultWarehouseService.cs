using Sales.API.DTOs;
using Sales.API.HttpClients;
using Sales.API.Models;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class DefaultWarehouseService
{
    private readonly IDefaultWarehouseRepository _defaults;
    private readonly InventoryClient _inventory;
    private readonly ISalesUnitOfWork _uow;

    public DefaultWarehouseService(
        IDefaultWarehouseRepository defaults,
        InventoryClient inventory,
        ISalesUnitOfWork uow)
    {
        _defaults = defaults;
        _inventory = inventory;
        _uow = uow;
    }

    /// <summary>
    /// Resolves the warehouse a ticket should be bound to:
    /// explicit request → configured company default → first active Inventory warehouse → error.
    /// </summary>
    public async Task<string> ResolveWarehouseAsync(string companyCen, string? requested)
    {
        if (!string.IsNullOrWhiteSpace(requested))
            return requested;

        var configured = await _defaults.GetByCompanyAsync(companyCen);
        if (configured != null && !string.IsNullOrWhiteSpace(configured.WarehouseCen))
            return configured.WarehouseCen;

        var warehouses = await _inventory.GetWarehousesAsync(companyCen);
        var fallback = warehouses?.FirstOrDefault(w => w.IsActive) ?? warehouses?.FirstOrDefault();
        if (fallback != null && !string.IsNullOrWhiteSpace(fallback.WarehouseCen))
            return fallback.WarehouseCen;

        throw new InvalidOperationException("Company has no warehouses.");
    }

    public async Task<DefaultWarehouseContractResponse?> GetDefaultAsync(string companyCen)
    {
        var row = await _defaults.GetByCompanyAsync(companyCen);
        return row == null
            ? null
            : new DefaultWarehouseContractResponse { CompanyCen = row.CompanyCen, WarehouseCen = row.WarehouseCen };
    }

    public async Task<DefaultWarehouseContractResponse> SetDefaultAsync(string companyCen, string warehouseCen)
    {
        if (string.IsNullOrWhiteSpace(warehouseCen))
            throw new InvalidOperationException("warehouseCen is required.");

        // The default must be a warehouse owned by the company.
        var warehouses = await _inventory.GetWarehousesAsync(companyCen);
        if (warehouses == null || warehouses.All(w => w.WarehouseCen != warehouseCen))
            throw new InvalidOperationException("Warehouse does not belong to this company.");

        var row = await _defaults.GetByCompanyAsync(companyCen);
        if (row == null)
            _defaults.Add(new DefaultWarehouse { CompanyCen = companyCen, WarehouseCen = warehouseCen });
        else
            row.WarehouseCen = warehouseCen;

        await _uow.SaveChangesAsync();
        return new DefaultWarehouseContractResponse { CompanyCen = companyCen, WarehouseCen = warehouseCen };
    }
}
