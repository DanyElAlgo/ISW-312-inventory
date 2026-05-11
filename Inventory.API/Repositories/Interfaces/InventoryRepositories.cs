using Inventory.API.Models;

namespace Inventory.API.Repositories.Interfaces;

public interface IBusinessRepository
{
    Task<IReadOnlyList<Business>> GetAllAsync();
    Task<Business?> ResolveAsync(string companyCen);
}

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetByBusinessIdAsync(int businessId);
    Task<Category?> ResolveAsync(int businessId, string categoryCen);
    Task<Category> AddAsync(Category category);
}

public interface IUnitRepository
{
    Task<IReadOnlyList<Unit>> GetByBusinessIdAsync(int businessId);
    Task<Unit?> ResolveAsync(int businessId, string unitCen);
    Task<Unit> AddAsync(Unit unit);
}

public interface IWarehouseRepository
{
    Task<IReadOnlyList<Warehouse>> GetByBusinessIdAsync(int businessId);
    Task<Warehouse?> ResolveAsync(int businessId, string warehouseCen);
    Task<Warehouse> AddAsync(Warehouse warehouse);
}

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetByBusinessIdAsync(int businessId, int? categoryId = null, string? search = null);
    Task<Product?> ResolveAsync(int businessId, string productCen);
    Task<Product> AddAsync(Product product);
}

public interface IWarehouseProductRepository
{
    Task<IReadOnlyList<WarehouseProduct>> GetByBusinessIdAsync(int businessId, int? productId = null, int? warehouseId = null);
    Task<WarehouseProduct?> ResolveAsync(int warehouseId, int productId);
    Task<decimal> GetCurrentStockAsync(int warehouseId, int productId);
    Task SetStatusForProductAsync(int productId, int statusId);
}