using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;

namespace InventoryAPI.Repositories;

public class WarehouseProductRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WarehouseProduct>> GetAllAsync()
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
            .Include(wp => wp.Warehouse)
            .Include(wp => wp.Status)
            .ToListAsync();
    }

    public async Task<WarehouseProduct?> GetByIdAsync(int id)
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
            .Include(wp => wp.Warehouse)
            .Include(wp => wp.Status)
            .FirstOrDefaultAsync(wp => wp.Id == id);
    }

    public async Task<IEnumerable<WarehouseProduct>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
            .Include(wp => wp.Warehouse)
            .Include(wp => wp.Status)
            .Where(wp => wp.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseProduct>> GetByProductIdAsync(int productId)
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
            .Include(wp => wp.Warehouse)
            .Include(wp => wp.Status)
            .Where(wp => wp.ProductId == productId)
            .ToListAsync();
    }

    public async Task<WarehouseProduct?> GetByWarehouseAndProductAsync(int warehouseId, int productId)
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
            .Include(wp => wp.Warehouse)
            .Include(wp => wp.Status)
            .FirstOrDefaultAsync(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId);
    }

    public async Task<WarehouseProduct> CreateAsync(WarehouseProduct warehouseProduct)
    {
        _context.WarehouseProducts.Add(warehouseProduct);
        await _context.SaveChangesAsync();
        return warehouseProduct;
    }

    public async Task<WarehouseProduct> UpdateAsync(WarehouseProduct warehouseProduct)
    {
        _context.WarehouseProducts.Update(warehouseProduct);
        await _context.SaveChangesAsync();
        return warehouseProduct;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var warehouseProduct = await _context.WarehouseProducts.FindAsync(id);
        if (warehouseProduct == null)
            return false;

        _context.WarehouseProducts.Remove(warehouseProduct);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.WarehouseProducts.AnyAsync(wp => wp.Id == id);
    }

    public async Task<IEnumerable<WarehouseProduct>> GetLowStockItemsAsync()
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
            .Include(wp => wp.Warehouse)
            .Include(wp => wp.Status)
            .Where(wp => wp.StockLeft < wp.LowStockQty)
            .ToListAsync();
    }
}
