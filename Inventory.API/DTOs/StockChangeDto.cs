using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs;

public class StockChangeDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public string ActionType { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public int? DestinationWarehouseId { get; set; }
}
