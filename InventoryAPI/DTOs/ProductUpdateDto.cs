namespace InventoryAPI.DTOs;

public class ProductUpdateDto
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? MetricUnit { get; set; }
    public bool? Status { get; set; }
    public int? Batch { get; set; }
    public int? LowStockQty { get; set; }
}
