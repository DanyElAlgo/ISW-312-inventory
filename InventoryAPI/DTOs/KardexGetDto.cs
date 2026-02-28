namespace InventoryAPI.DTOs;

public class KardexGetDto
{
    public int KardexId { get; set; }
    public int WarehousePrimaryId { get; set; }
    public int? WarehouseSecondaryId { get; set; }
    public int BusinessId { get; set; }
    public int ProductId { get; set; }
    public string ActionType { get; set; } = null!;
    public int ActionQty { get; set; }
    public string? Reason { get; set; }
    public DateTime? Timestamp { get; set; }
}
