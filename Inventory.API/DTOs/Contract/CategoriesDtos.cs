using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs.Contract;

public sealed class CategoryDto
{
    public required string CategoryCen { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateCategoryRequest
{
    [Required]
    public required string Name { get; init; }

    public string? Description { get; init; }
}

public sealed class UpdateCategoryRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
}