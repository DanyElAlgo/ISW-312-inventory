using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories.Ef;

internal static class InventoryCenBuilder
{
    public static string Build(string prefix, int id) => $"{prefix}-{id:D6}";
}

public sealed class BusinessRepository : IBusinessRepository
{
    private readonly InventoryDbContext _context;

    public BusinessRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Business>> GetAllAsync()
    {
        var businesses = await _context.Businesses
            .OrderBy(b => b.Name)
            .ToListAsync();

        var updated = false;
        foreach (var business in businesses)
        {
            if (string.IsNullOrWhiteSpace(business.Cen))
            {
                business.Cen = InventoryCenBuilder.Build("COMP", business.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return businesses;
    }

    public async Task<Business?> ResolveAsync(string companyCen)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Cen == companyCen);

        if (business == null && int.TryParse(companyCen, out var id))
            business = await _context.Businesses.FindAsync(id);

        if (business != null && string.IsNullOrWhiteSpace(business.Cen))
        {
            business.Cen = InventoryCenBuilder.Build("COMP", business.Id);
            await _context.SaveChangesAsync();
        }

        return business;
    }
}

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly InventoryDbContext _context;

    public CategoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetByBusinessIdAsync(int businessId)
    {
        var categories = await _context.Categories
            .Where(c => c.BusinessId == businessId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var updated = false;
        foreach (var category in categories)
        {
            if (string.IsNullOrWhiteSpace(category.Cen))
            {
                category.Cen = InventoryCenBuilder.Build("CAT", category.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return categories;
    }

    public async Task<Category?> ResolveAsync(int businessId, string categoryCen)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Cen == categoryCen);

        if (category == null && int.TryParse(categoryCen, out var id))
            category = await _context.Categories.FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);

        if (category != null && string.IsNullOrWhiteSpace(category.Cen))
        {
            category.Cen = InventoryCenBuilder.Build("CAT", category.Id);
            await _context.SaveChangesAsync();
        }

        return category;
    }

    public Task<Category> AddAsync(Category category)
    {
        return Task.FromResult(_context.Categories.Add(category).Entity);
    }
}

public sealed class UnitRepository : IUnitRepository
{
    private readonly InventoryDbContext _context;

    public UnitRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Unit>> GetByBusinessIdAsync(int businessId)
    {
        var units = await _context.Units
            .Where(u => u.BusinessId == businessId)
            .OrderBy(u => u.Name)
            .ToListAsync();

        var updated = false;
        foreach (var unit in units)
        {
            if (string.IsNullOrWhiteSpace(unit.Cen))
            {
                unit.Cen = InventoryCenBuilder.Build("UNIT", unit.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return units;
    }

    public async Task<Unit?> ResolveAsync(int businessId, string unitCen)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.BusinessId == businessId && u.Cen == unitCen);

        if (unit == null && int.TryParse(unitCen, out var id))
            unit = await _context.Units.FirstOrDefaultAsync(u => u.BusinessId == businessId && u.Id == id);

        if (unit != null && string.IsNullOrWhiteSpace(unit.Cen))
        {
            unit.Cen = InventoryCenBuilder.Build("UNIT", unit.Id);
            await _context.SaveChangesAsync();
        }

        return unit;
    }

    public Task<Unit> AddAsync(Unit unit)
    {
        return Task.FromResult(_context.Units.Add(unit).Entity);
    }
}

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Warehouse>> GetByBusinessIdAsync(int businessId)
    {
        var warehouses = await _context.Warehouses
            .Where(w => w.BusinessId == businessId)
            .OrderBy(w => w.Name)
            .ToListAsync();

        var updated = false;
        foreach (var warehouse in warehouses)
        {
            if (string.IsNullOrWhiteSpace(warehouse.Cen))
            {
                warehouse.Cen = InventoryCenBuilder.Build("WH", warehouse.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return warehouses;
    }

    public async Task<Warehouse?> ResolveAsync(int businessId, string warehouseCen)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.BusinessId == businessId && w.Cen == warehouseCen);

        if (warehouse == null && int.TryParse(warehouseCen, out var id))
            warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.BusinessId == businessId && w.Id == id);

        if (warehouse != null && string.IsNullOrWhiteSpace(warehouse.Cen))
        {
            warehouse.Cen = InventoryCenBuilder.Build("WH", warehouse.Id);
            await _context.SaveChangesAsync();
        }

        return warehouse;
    }

    public Task<Warehouse> AddAsync(Warehouse warehouse)
    {
        return Task.FromResult(_context.Warehouses.Add(warehouse).Entity);
    }
}

public sealed class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetByBusinessIdAsync(int businessId, int? categoryId = null, string? search = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.WarehouseProducts)
            .Where(p => p.BusinessId == businessId)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(searchTerm)) ||
                (p.Sku != null && p.Sku.ToLower().Contains(searchTerm)) ||
                (p.Cen != null && p.Cen.ToLower().Contains(searchTerm)));
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        var updated = false;

        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Cen))
            {
                product.Cen = string.IsNullOrWhiteSpace(product.Sku)
                    ? InventoryCenBuilder.Build("PROD", product.Id)
                    : product.Sku;
                updated = true;
            }

            if (product.Category != null && string.IsNullOrWhiteSpace(product.Category.Cen))
            {
                product.Category.Cen = InventoryCenBuilder.Build("CAT", product.Category.Id);
                updated = true;
            }

            if (product.Unit != null && string.IsNullOrWhiteSpace(product.Unit.Cen))
            {
                product.Unit.Cen = InventoryCenBuilder.Build("UNIT", product.Unit.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return products;
    }

    public async Task<Product?> ResolveAsync(int businessId, string productCen)
    {
        var product = await _context.Products
            .Include(p => p.Unit)
            .Include(p => p.Category)
            .Include(p => p.WarehouseProducts)
            .FirstOrDefaultAsync(p => p.BusinessId == businessId && (p.Cen == productCen || p.Sku == productCen));

        if (product == null && int.TryParse(productCen, out var id))
            product = await _context.Products
                .Include(p => p.Unit)
                .Include(p => p.Category)
                .Include(p => p.WarehouseProducts)
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.Id == id);

        var updated = false;
        if (product != null && string.IsNullOrWhiteSpace(product.Cen))
        {
            product.Cen = string.IsNullOrWhiteSpace(product.Sku)
                ? InventoryCenBuilder.Build("PROD", product.Id)
                : product.Sku;
            updated = true;
        }

        if (product != null && product.Category != null && string.IsNullOrWhiteSpace(product.Category.Cen))
        {
            product.Category.Cen = InventoryCenBuilder.Build("CAT", product.Category.Id);
            updated = true;
        }

        if (product != null && product.Unit != null && string.IsNullOrWhiteSpace(product.Unit.Cen))
        {
            product.Unit.Cen = InventoryCenBuilder.Build("UNIT", product.Unit.Id);
            updated = true;
        }

        if (product != null && updated)
            await _context.SaveChangesAsync();

        return product;
    }

    public Task<Product> AddAsync(Product product)
    {
        return Task.FromResult(_context.Products.Add(product).Entity);
    }
}

public sealed class WarehouseProductRepository : IWarehouseProductRepository
{
    private readonly InventoryDbContext _context;

    public WarehouseProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WarehouseProduct>> GetByBusinessIdAsync(int businessId, int? productId = null, int? warehouseId = null)
    {
        var query = _context.WarehouseProducts
            .Include(wp => wp.Product)
                .ThenInclude(p => p!.Unit)
            .Include(wp => wp.Warehouse)
            .Where(wp => wp.Warehouse!.BusinessId == businessId)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(wp => wp.ProductId == productId);

        if (warehouseId.HasValue)
            query = query.Where(wp => wp.WarehouseId == warehouseId);

        var items = await query.ToListAsync();
        var updated = false;

        foreach (var item in items)
        {
            if (item.Product != null && string.IsNullOrWhiteSpace(item.Product.Cen))
            {
                item.Product.Cen = string.IsNullOrWhiteSpace(item.Product.Sku)
                    ? InventoryCenBuilder.Build("PROD", item.Product.Id)
                    : item.Product.Sku;
                updated = true;
            }

            if (item.Warehouse != null && string.IsNullOrWhiteSpace(item.Warehouse.Cen))
            {
                item.Warehouse.Cen = InventoryCenBuilder.Build("WH", item.Warehouse.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return items;
    }

    public async Task<WarehouseProduct?> ResolveAsync(int warehouseId, int productId)
    {
        return await _context.WarehouseProducts
            .Include(wp => wp.Product)
                .ThenInclude(p => p!.Unit)
            .Include(wp => wp.Warehouse)
            .FirstOrDefaultAsync(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId);
    }

    public async Task<decimal> GetCurrentStockAsync(int warehouseId, int productId)
    {
        return await _context.WarehouseProducts
            .Where(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId)
            .Select(wp => wp.StockLeft ?? 0)
            .FirstOrDefaultAsync();
    }

    public async Task SetStatusForProductAsync(int productId, int statusId)
    {
        var items = await _context.WarehouseProducts.Where(wp => wp.ProductId == productId).ToListAsync();
        foreach (var item in items)
            item.StatusId = statusId;
    }
}