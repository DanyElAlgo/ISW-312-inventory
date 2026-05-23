using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs.Contract;

public sealed class ProductDto
{
    public required string ProductCen { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string CategoryCen { get; init; }
    public required string CategoryName { get; init; }
    public required string UnitCen { get; init; }
    public required string UnitName { get; init; }
    public decimal SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int ReorderLevel { get; init; }
    public required string Status { get; init; }
    public string? StationCode { get; init; }
}

public sealed class CreateProductRequest
{
    [Required]
    public required string Sku { get; init; }

    [Required]
    public required string Name { get; init; }

    public string? Description { get; init; }

    [Required]
    public required string CategoryCen { get; init; }

    [Required]
    public required string UnitCen { get; init; }

    public decimal SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int ReorderLevel { get; init; }
    public string? StationCode { get; init; }
}

public sealed class UpdateProductRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? CategoryCen { get; init; }
    public string? UnitCen { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int? ReorderLevel { get; init; }
    public string? StationCode { get; init; }
}

public sealed class UpdateProductStatusRequest
{
    [Required]
    public required string Status { get; init; }

    public string? Reason { get; init; }
}

public sealed class ProductLookupContractRequest
{
    public IReadOnlyList<string> ProductCens { get; init; } = new List<string>();
}

public sealed class SellableProductDto
{
    public required string ProductCen { get; init; }
    public required string Name { get; init; }
    public required string CategoryCen { get; init; }
    public required string CategoryName { get; init; }
    public decimal SalePrice { get; init; }
    public decimal AvailableQuantity { get; init; }
    public bool IsAvailable { get; init; }
    public string? StationCode { get; init; }
}