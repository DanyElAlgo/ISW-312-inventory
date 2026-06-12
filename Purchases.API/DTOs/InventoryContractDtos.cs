using System.ComponentModel.DataAnnotations;

namespace Purchases.API.DTOs;

// Mirrors of the DTOs Purchases uses to talk to Inventory.API.
// See backend/Inventory.API/DTOs/Contract/StockDtos.cs for the source shapes.

public sealed class StockIncreaseRequest
{
    [Required]
    public required string WarehouseCen { get; init; }

    [Required]
    public required string Source { get; init; }

    public string? ReferenceCen { get; init; }

    public string? Reason { get; init; }

    [Required]
    public required IReadOnlyList<StockIncreaseItemDto> Items { get; init; }
}

public sealed class StockIncreaseItemDto
{
    [Required]
    public required string ProductCen { get; init; }

    public decimal Quantity { get; init; }
}

public sealed class StockIncreaseResponse
{
    public string? DocumentCen { get; init; }
    public string? DocumentType { get; init; }
    public IReadOnlyList<string> GeneratedMovementCens { get; init; } = [];
}

public sealed class InventoryProductDto
{
    public required string ProductCen { get; init; }
    public required string Name { get; init; }
    public string? Sku { get; init; }
    public string? CategoryCen { get; init; }
    public string? UnitCen { get; init; }
    public string? UnitName { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
}
