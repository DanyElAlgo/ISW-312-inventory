namespace InventoryAPI.DTOs;

public class WarehouseProductGetDto
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
    public bool IsLowStock => StockLeft.HasValue && LowStockQty.HasValue && StockLeft < LowStockQty;
}

public class WarehouseProductCreateDto
{
    public required int WarehouseId { get; set; }
    public required int ProductId { get; set; }
    public int? StatusId { get; set; }
    public int StockLeft { get; set; }
    public int? LowStockQty { get; set; }
}

public class WarehouseProductUpdateDto
{
    public int? StatusId { get; set; }
    public int? StockLeft { get; set; }
    public int? LowStockQty { get; set; }
}
