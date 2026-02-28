namespace InventoryAPI.DTOs;

public class ProductCreateDto
{
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MetricUnit { get; set; } = null!;
    public bool Status { get; set; } = true;
    public int? Batch { get; set; }
    public int InitialStock { get; set; } = 0;
    public int? LowStockQty { get; set; }
}
