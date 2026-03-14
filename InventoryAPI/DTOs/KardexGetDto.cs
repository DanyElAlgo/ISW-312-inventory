namespace InventoryAPI.DTOs;

public class KardexGetDto
{
    public int Id { get; set; }
    public int? WarehouseId { get; set; }
    public int? ProductId { get; set; }
    public string? ActionType { get; set; }
    public double? ActionQty { get; set; }
    public string? Reason { get; set; }
    public DateTime? TimeStamp { get; set; }
}
