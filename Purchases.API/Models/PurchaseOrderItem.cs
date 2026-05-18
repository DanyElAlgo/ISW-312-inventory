namespace Purchases.API.Models;

public partial class PurchaseOrderItem
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    public string ProductCen { get; set; } = string.Empty;

    public string? ProductName { get; set; }

    public int Quantity { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }
}
