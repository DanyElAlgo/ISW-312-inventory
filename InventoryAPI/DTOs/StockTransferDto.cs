namespace InventoryAPI.DTOs;

public class StockTransferDto
{
    public int ProductId { get; set; }
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}
