namespace Shared.Contracts.DTOs;

public class BulkStockCheckItemDto
{
    public int ProductId { get; set; }
    public double Quantity { get; set; }
}

public class BulkStockCheckDto
{
    public List<BulkStockCheckItemDto> Items { get; set; } = new();
}

public class StockCheckLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double Required { get; set; }
    public int Available { get; set; }
    public bool Sufficient { get; set; }
}

public class BulkStockCheckResultDto
{
    public bool AllAvailable { get; set; }
    public List<StockCheckLineDto> Lines { get; set; } = new();
}

public class BulkStockDeductDto
{
    public List<BulkStockCheckItemDto> Items { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

public class BulkStockDeductResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class WarehouseProductDto
{
    public int Id { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public int? StockLeft { get; set; }
    public int? LowStockQty { get; set; }
}
