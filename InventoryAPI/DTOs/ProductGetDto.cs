namespace InventoryAPI.DTOs;

public class ProductGetDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MetricUnit { get; set; } = null!;
    public bool Status { get; set; }
    public int? Batch { get; set; }
    public int StockLeft { get; set; }
    public int? LowStockQty { get; set; }
}
