using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;

namespace Inventory.API.Services;

public class CategoriesService : InventoryServiceBase
{
    public CategoriesService(
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

    public async Task<IReadOnlyList<CategoryDto>?> GetCategoriesAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var categories = await CategoryRepository.GetByBusinessIdAsync(business.Id);

        return categories.Select(MapCategory).ToList();
    }

    public async Task<CategoryDto?> CreateCategoryAsync(string companyCen, CreateCategoryRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Category name is required.");

        var category = new Category
        {
            BusinessId = business.Id,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            IsActive = true
        };

        await CategoryRepository.AddAsync(category);
        await Context.SaveChangesAsync();

        category.Cen = BuildCen("CAT", category.Id);
        await Context.SaveChangesAsync();

        return MapCategory(category);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(string companyCen, string categoryCen, UpdateCategoryRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var category = await ResolveCategoryAsync(business.Id, categoryCen);
        if (category == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            category.Name = dto.Name.Trim();

        if (dto.Description != null)
            category.Description = dto.Description;

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        await Context.SaveChangesAsync();
        return MapCategory(category);
    }
}
