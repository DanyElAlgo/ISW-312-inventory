using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;
using InventoryAPI.DTOs;

namespace InventoryAPI.Repositories;

public class ProductSearchRepository
{
    private readonly InventoryDbContext _context;

    public ProductSearchRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ProductSearchResultDto> items, int totalCount)> SearchAsync(
        string? searchTerm = null,
        int? categoryId = null,
        int? statusId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.WarehouseProducts)
            .ThenInclude(wp => wp.Status)
            .AsQueryable();

        // Apply search term filter (partial match on name and description)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name != null && p.Name.ToLower().Contains(lowerSearchTerm) ||
                p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // Apply status filter (filter by warehouse product status)
        if (statusId.HasValue)
        {
            query = query.Where(p => p.WarehouseProducts.Any(wp => wp.StatusId == statusId));
        }

        var totalCount = await query.CountAsync();

        var results = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductSearchResultDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                UnitId = p.UnitId,
                UnitName = p.Unit != null ? p.Unit.Name : null,
                UnitQty = p.UnitQty,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                TotalStock = p.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0),
                LowStockCount = p.WarehouseProducts.Count(wp => wp.StockLeft < wp.LowStockQty)
            })
            .ToListAsync();

        return (results, totalCount);
    }
}
