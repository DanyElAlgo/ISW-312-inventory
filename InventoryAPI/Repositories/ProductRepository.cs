using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;

namespace InventoryAPI.Repositories;

public class ProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(Product item)
    {
        _context.Products.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<Product> UpdateAsync(Product item)
    {
        _context.Products.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.Products.FindAsync(id);
        if (item == null)
            return false;

        _context.Products.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(i => i.Id == id);
    }

    public async Task<Kardex> AddKardexEntryAsync(Kardex kardexEntry)
    {
        _context.Kardices.Add(kardexEntry);
        await _context.SaveChangesAsync();
        return kardexEntry;
    }

    public async Task<IEnumerable<Kardex>> GetKardexByProductIdAsync(int productId)
    {
        return await _context.Kardices
            .Where(k => k.ProductId == productId)
            .OrderByDescending(k => k.TimeStamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Kardex>> GetKardexByWarehouseAsync(int warehouseId)
    {
        return await _context.Kardices
            .Where(k => k.WarehouseId == warehouseId)
            .OrderByDescending(k => k.TimeStamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Kardex>> GetKardexByProductAndWarehouseAsync(int productId, int warehouseId)
    {
        return await _context.Kardices
            .Where(k => k.ProductId == productId && k.WarehouseId == warehouseId)
            .OrderByDescending(k => k.TimeStamp)
            .ToListAsync();
    }
}

