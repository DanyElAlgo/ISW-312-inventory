using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;

namespace InventoryAPI.Repositories;

public class BusinessRepository
{
    private readonly InventoryDbContext _context;

    public BusinessRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        return await _context.Businesses
            .Include(b => b.Warehouses)
            .ToListAsync();
    }

    public async Task<Business?> GetByIdAsync(int id)
    {
        return await _context.Businesses
            .Include(b => b.Warehouses)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Business> CreateAsync(Business business)
    {
        _context.Businesses.Add(business);
        await _context.SaveChangesAsync();
        return business;
    }

    public async Task<Business> UpdateAsync(Business business)
    {
        _context.Businesses.Update(business);
        await _context.SaveChangesAsync();
        return business;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business == null)
            return false;

        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Businesses.AnyAsync(w => w.Id == id);
    }
}
