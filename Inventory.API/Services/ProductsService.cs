using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;

namespace Inventory.API.Services;

public class ProductsService : InventoryServiceBase
{
    public ProductsService(
        InventoryDbContext context,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitRepository unitRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseProductRepository warehouseProductRepository)
        : base(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository)
    {
    }

    public async Task<IReadOnlyList<ProductDto>?> GetProductsAsync(
        string companyCen,
        string? search,
        string? categoryCen,
        string? status)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        int? categoryId = null;
        if (!string.IsNullOrWhiteSpace(categoryCen))
        {
            var category = await ResolveCategoryAsync(business.Id, categoryCen);
            if (category == null)
                return null;

            categoryId = category.Id;
        }

        var products = await ProductRepository.GetByBusinessIdAsync(business.Id, categoryId, search);

        var results = products.Select(p => MapProduct(p, GetProductStatus(p))).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToUpperInvariant();
            results = results.Where(p => p.Status == normalized).ToList();
        }

        return results;
    }

    public async Task<IReadOnlyList<ProductDto>?> LookupProductsAsync(string companyCen, IReadOnlyList<string> productCens)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (productCens == null || productCens.Count == 0)
            return new List<ProductDto>();

        var wanted = new HashSet<string>(productCens);
        var products = await ProductRepository.GetByBusinessIdAsync(business.Id);

        return products
            .Where(p => p.Cen != null && wanted.Contains(p.Cen))
            .Select(p => MapProduct(p, GetProductStatus(p)))
            .ToList();
    }

    public async Task<ProductDto?> GetProductAsync(string companyCen, string productCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<ProductDto?> CreateProductAsync(string companyCen, CreateProductRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Product name is required.");

        if (string.IsNullOrWhiteSpace(dto.Sku))
            throw new InvalidOperationException("SKU is required.");

        if (dto.SalePrice <= 0)
            throw new InvalidOperationException("Sale price must be greater than 0.");

        var category = await ResolveCategoryAsync(business.Id, dto.CategoryCen);
        if (category == null)
            throw new InvalidOperationException("Category not found for company.");

        var unit = await ResolveUnitAsync(business.Id, dto.UnitCen);
        if (unit == null)
            throw new InvalidOperationException("Unit not found for company.");

        var product = new Product
        {
            BusinessId = business.Id,
            Cen = dto.Sku.Trim(),
            Sku = dto.Sku.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            CategoryId = category.Id,
            UnitId = unit.Id,
            Price = dto.SalePrice,
            CostPrice = dto.CostPrice,
            ReorderLevel = dto.ReorderLevel,
            StationCode = dto.StationCode,
            IsActive = true
        };

        await ProductRepository.AddAsync(product);
        await Context.SaveChangesAsync();

        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<ProductDto?> UpdateProductAsync(string companyCen, string productCen, UpdateProductRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            product.Name = dto.Name.Trim();

        if (dto.Description != null)
            product.Description = dto.Description;

        if (!string.IsNullOrWhiteSpace(dto.CategoryCen))
        {
            var category = await ResolveCategoryAsync(business.Id, dto.CategoryCen);
            if (category == null)
                throw new InvalidOperationException("Category not found for company.");

            product.CategoryId = category.Id;
        }

        if (!string.IsNullOrWhiteSpace(dto.UnitCen))
        {
            var unit = await ResolveUnitAsync(business.Id, dto.UnitCen);
            if (unit == null)
                throw new InvalidOperationException("Unit not found for company.");

            product.UnitId = unit.Id;
        }

        if (dto.SalePrice.HasValue)
        {
            if (dto.SalePrice.Value <= 0)
                throw new InvalidOperationException("Sale price must be greater than 0.");

            product.Price = dto.SalePrice.Value;
        }

        if (dto.CostPrice.HasValue)
            product.CostPrice = dto.CostPrice;

        if (dto.ReorderLevel.HasValue)
            product.ReorderLevel = dto.ReorderLevel.Value;

        if (dto.StationCode != null)
            product.StationCode = dto.StationCode;

        await Context.SaveChangesAsync();
        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<ProductDto?> UpdateProductStatusAsync(string companyCen, string productCen, UpdateProductStatusRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        var status = dto.Status.Trim().ToUpperInvariant();
        if (status is "ACTIVE")
        {
            product.IsActive = true;
            await SetWarehouseProductStatusAsync(product.Id, ActiveStatusId);
        }
        else if (status is "INACTIVE")
        {
            product.IsActive = false;
        }
        else if (status is "OUT_OF_STOCK")
        {
            product.IsActive = true;
            await SetWarehouseProductStatusAsync(product.Id, OutOfStockStatusId);
        }
        else
        {
            throw new InvalidOperationException("Invalid status value.");
        }

        await Context.SaveChangesAsync();
        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<IReadOnlyList<SellableProductDto>?> GetSellableProductsAsync(
        string companyCen,
        string? search,
        string? categoryCen,
        string? warehouseCen,
        bool onlyAvailable,
        int page,
        int pageSize)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        int? categoryId = null;
        if (!string.IsNullOrWhiteSpace(categoryCen))
        {
            var category = await ResolveCategoryAsync(business.Id, categoryCen);
            if (category == null)
                return null;

            categoryId = category.Id;
        }

        int? warehouseId = null;
        if (!string.IsNullOrWhiteSpace(warehouseCen))
        {
            var warehouse = await ResolveWarehouseAsync(business.Id, warehouseCen);
            if (warehouse == null)
                return null;

            warehouseId = warehouse.Id;
        }

        var products = await ProductRepository.GetByBusinessIdAsync(business.Id, categoryId, search);

        var results = products
            .Where(p => p.IsActive.GetValueOrDefault(true))
            .Select(p =>
            {
                var stockEntries = warehouseId.HasValue
                    ? p.WarehouseProducts.Where(wp => wp.WarehouseId == warehouseId.Value)
                    : p.WarehouseProducts;
                var available = stockEntries.Sum(wp => wp.StockLeft ?? 0);

                return new SellableProductDto
                {
                    ProductCen = p.Cen ?? BuildCen("PRD", p.Id),
                    Name = p.Name ?? string.Empty,
                    CategoryCen = p.Category?.Cen ?? string.Empty,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    SalePrice = p.Price ?? 0,
                    AvailableQuantity = available,
                    IsAvailable = available > 0,
                    StationCode = p.StationCode
                };
            });

        if (onlyAvailable)
            results = results.Where(p => p.IsAvailable);

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 50 : pageSize;

        return results
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();
    }

    private async Task SetWarehouseProductStatusAsync(int productId, int statusId)
    {
        await WarehouseProductRepository.SetStatusForProductAsync(productId, statusId);
    }
}
