namespace InventoryAPI.DTOs;

public class StockSetDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
}
