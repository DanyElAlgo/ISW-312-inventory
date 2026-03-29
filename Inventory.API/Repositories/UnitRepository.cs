using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;

namespace Inventory.API.Repositories;

public class UnitRepository
{
    private readonly InventoryDbContext _context;

    public UnitRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Unit>> GetAllAsync()
    {
        return await _context.Units
            .Include(u => u.Products)
            .ToListAsync();
    }

    public async Task<Unit?> GetByIdAsync(int id)
    {
        return await _context.Units
            .Include(u => u.Products)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Unit> CreateAsync(Unit unit)
    {
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();
        return unit;
    }

    public async Task<Unit> UpdateAsync(Unit unit)
    {
        _context.Units.Update(unit);
        await _context.SaveChangesAsync();
        return unit;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
            return false;

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Units.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludingId = null)
    {
        var normalizedName = name.Trim().ToLower();
        return await _context.Units.AnyAsync(u =>
            u.Name != null &&
            u.Name.ToLower() == normalizedName &&
            (!excludingId.HasValue || u.Id != excludingId.Value));
    }
}
