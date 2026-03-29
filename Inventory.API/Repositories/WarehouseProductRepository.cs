using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;

namespace Inventory.API.Repositories;

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

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await _context.Products.AnyAsync(p => p.Id == productId);
    }

    public async Task<bool> WarehouseExistsAsync(int warehouseId)
    {
        return await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
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

    public async Task<int> GetTotalStockByProductAsync(int productId)
    {
        return await _context.WarehouseProducts
            .Where(wp => wp.ProductId == productId)
            .SumAsync(wp => wp.StockLeft ?? 0);
    }

    public async Task<int?> GetFirstWarehouseWithStockAsync(int productId, int quantity)
    {
        var wp = await _context.WarehouseProducts
            .Where(wp => wp.ProductId == productId && (wp.StockLeft ?? 0) >= quantity)
            .FirstOrDefaultAsync();
        return wp?.WarehouseId;
    }

    public async Task<string?> GetProductNameAsync(int productId)
    {
        return await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync();
    }

    public async Task<Kardex> AddKardexEntryAsync(Kardex kardexEntry)
    {
        _context.Kardices.Add(kardexEntry);
        await _context.SaveChangesAsync();
        return kardexEntry;
    }
}
