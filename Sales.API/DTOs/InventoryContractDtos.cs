using System.ComponentModel.DataAnnotations;

namespace Sales.API.DTOs;

public sealed class InventoryProductDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? StationCode { get; set; }
}

public sealed class InventoryStockItemDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
}

public sealed class StockValidationRequest
{
    [Required]
    public string WarehouseCen { get; set; } = string.Empty;
    [Required]
    public string Source { get; set; } = string.Empty;
    public string? ReferenceCen { get; set; }
    [Required]
    public List<StockValidationItemDto> Items { get; set; } = new();
}

public sealed class StockValidationItemDto
{
    [Required]
    public string ProductCen { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public sealed class StockRequirementDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal MissingQuantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public sealed class StockValidationResponse
{
    public bool IsValid { get; set; }
    public List<StockRequirementDto> Requirements { get; set; } = new();
}

public sealed class StockConsumeRequest
{
    [Required]
    public string WarehouseCen { get; set; } = string.Empty;
    [Required]
    public string Source { get; set; } = string.Empty;
    [Required]
    public string ReferenceCen { get; set; } = string.Empty;
    public string? Reason { get; set; }
    [Required]
    public List<StockConsumeItemDto> Items { get; set; } = new();
}

public sealed class StockConsumeItemDto
{
    [Required]
    public string ProductCen { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public sealed class StockConsumeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? DocumentCen { get; set; }
    public string? DocumentType { get; set; }
    public List<string> GeneratedMovementCens { get; set; } = new();
    public List<StockRequirementDto> Requirements { get; set; } = new();
}
