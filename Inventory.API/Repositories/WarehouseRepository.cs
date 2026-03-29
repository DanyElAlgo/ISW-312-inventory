using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;

namespace Inventory.API.Repositories;

public class WarehouseRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses
            .Include(w => w.Business)
            .Include(w => w.WarehouseProducts)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetByIdAsync(int id)
    {
        return await _context.Warehouses
            .Include(w => w.Business)
            .Include(w => w.WarehouseProducts)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Warehouse>> GetByBusinessIdAsync(int businessId)
    {
        return await _context.Warehouses
            .Include(w => w.Business)
            .Include(w => w.WarehouseProducts)
            .Where(w => w.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse)
    {
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
            return false;

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Warehouses.AnyAsync(w => w.Id == id);
    }
}
