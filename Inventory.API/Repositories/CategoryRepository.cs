using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;

namespace Inventory.API.Repositories;

public class CategoryRepository
{
    private readonly InventoryDbContext _context;

    public CategoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludingId = null)
    {
        var normalizedName = name.Trim().ToLower();
        return await _context.Categories.AnyAsync(c =>
            c.Name != null &&
            c.Name.ToLower() == normalizedName &&
            (!excludingId.HasValue || c.Id != excludingId.Value));
    }
}
