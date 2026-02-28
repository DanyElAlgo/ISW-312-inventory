namespace InventoryAPI.DTOs;

public class StockChangeDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public int? DestinationWarehouseId { get; set; }
}
