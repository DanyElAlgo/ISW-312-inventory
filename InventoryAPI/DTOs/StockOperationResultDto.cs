namespace InventoryAPI.DTOs;

public class StockOperationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? NewStock { get; set; }
    public int? KardexId { get; set; }
}
