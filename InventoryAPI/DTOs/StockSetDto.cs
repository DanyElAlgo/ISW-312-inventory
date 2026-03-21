using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.DTOs;

public class StockSetDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public string? Reason { get; set; }
}
