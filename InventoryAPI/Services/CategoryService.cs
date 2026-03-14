using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class CategoryService
{
    private readonly CategoryRepository _repository;

    public CategoryService(CategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryGetDto>> GetAllCategoriesAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(MapToGetDto);
    }

    public async Task<CategoryGetDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category == null ? null : MapToGetDto(category);
    }

    public async Task<CategoryGetDto> CreateCategoryAsync(CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        var createdCategory = await _repository.CreateAsync(category);
        return MapToGetDto(createdCategory);
    }

    public async Task<CategoryGetDto?> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            category.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            category.Description = dto.Description;

        var updatedCategory = await _repository.UpdateAsync(category);
        return MapToGetDto(updatedCategory);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static CategoryGetDto MapToGetDto(Category category)
    {
        return new CategoryGetDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = category.Products?.Count
        };
    }
}
