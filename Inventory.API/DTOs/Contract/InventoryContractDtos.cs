using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs.Contract;

public sealed class CompanyDto
{
    public required string CompanyCen { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}

public sealed class InventoryDashboardDto
{
    public required string CompanyCen { get; init; }
    public int TotalProducts { get; init; }
    public int TotalStockQuantity { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
}

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

public sealed class UnitDto
{
    public required string UnitCen { get; init; }
    public required string Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateUnitRequest
{
    [Required]
    public required string Name { get; init; }
    public string? Abbreviation { get; init; }
}

public sealed class UpdateUnitRequest
{
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class WarehouseDto
{
    public required string WarehouseCen { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateWarehouseRequest
{
    [Required]
    public required string Name { get; init; }
}

public sealed class UpdateWarehouseRequest
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
}

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

public sealed class StockItemDto
{
    public required string ProductCen { get; init; }
    public required string ProductName { get; init; }
    public required string WarehouseCen { get; init; }
    public required string WarehouseName { get; init; }
    public decimal AvailableQuantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public required string UnitName { get; init; }
    public decimal ReorderLevel { get; init; }
    public bool IsLowStock { get; init; }
}

public sealed class StockAdjustmentRequest
{
    [Required]
    public required string WarehouseCen { get; init; }
    [Required]
    public required string Reason { get; init; }
    [Required]
    public required IReadOnlyList<StockAdjustmentLineDto> Lines { get; init; }
}

public sealed class StockAdjustmentLineDto
{
    [Required]
    public required string ProductCen { get; init; }
    public decimal Quantity { get; init; }
    [Required]
    public required string AdjustmentType { get; init; }
}

public sealed class InventoryMovementDto
{
    public required string MovementCen { get; init; }
    public required string ProductCen { get; init; }
    public required string WarehouseCen { get; init; }
    public decimal Quantity { get; init; }
    public required string MovementType { get; init; }
}

public sealed class StockAdjustmentResponse
{
    public required string AdjustmentCen { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyList<InventoryMovementDto> GeneratedMovements { get; init; }
}

public sealed class KardexMovementDto
{
    public required string MovementCen { get; init; }
    public string? DocumentCen { get; init; }
    public required string ProductCen { get; init; }
    public required string WarehouseCen { get; init; }
    public required string MovementType { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitCost { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class InventoryDocumentCreateRequest
{
    [Required]
    public required string DocumentType { get; init; }
    [Required]
    public required string WarehouseCen { get; init; }
    public string? Reason { get; init; }
    public string? ExternalReference { get; init; }
    public string? Source { get; init; }
    public string? ReferenceCen { get; init; }
    [Required]
    public required IReadOnlyList<InventoryDocumentLineRequest> Lines { get; init; }
}

public sealed class InventoryDocumentLineRequest
{
    [Required]
    public required string ProductCen { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitCost { get; init; }
}

public sealed class InventoryDocumentDto
{
    public required string DocumentCen { get; init; }
    public required string DocumentType { get; init; }
    public required string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public int TotalItems { get; init; }
    public IReadOnlyList<string> GeneratedMovementCens { get; init; } = [];
}

public sealed class StockValidationRequest
{
    [Required]
    public required string WarehouseCen { get; init; }
    [Required]
    public required string Source { get; init; }
    public string? ReferenceCen { get; init; }
    [Required]
    public required IReadOnlyList<StockValidationItemDto> Items { get; init; }
}

public sealed class StockValidationItemDto
{
    [Required]
    public required string ProductCen { get; init; }
    public decimal Quantity { get; init; }
}

public sealed class StockRequirementDto
{
    public required string ProductCen { get; init; }
    public required string ProductName { get; init; }
    public required string WarehouseCen { get; init; }
    public decimal RequestedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public decimal MissingQuantity { get; init; }
    public required string UnitName { get; init; }
    public required string Reason { get; init; }
}

public sealed class StockValidationResponse
{
    public bool IsValid { get; init; }
    public required IReadOnlyList<StockRequirementDto> Requirements { get; init; }
}

public sealed class StockConsumeRequest
{
    [Required]
    public required string WarehouseCen { get; init; }
    [Required]
    public required string Source { get; init; }
    [Required]
    public required string ReferenceCen { get; init; }
    public string? Reason { get; init; }
    [Required]
    public required IReadOnlyList<StockConsumeItemDto> Items { get; init; }
}

public sealed class StockConsumeItemDto
{
    [Required]
    public required string ProductCen { get; init; }
    public decimal Quantity { get; init; }
}

public sealed class StockConsumeResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? DocumentCen { get; init; }
    public string? DocumentType { get; init; }
    public IReadOnlyList<string> GeneratedMovementCens { get; init; } = [];
    public IReadOnlyList<StockRequirementDto> Requirements { get; init; } = [];
}
