namespace Purchases.API.Models;

public partial class PurchaseOrder
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public int SupplierId { get; set; }

    public string WarehouseCen { get; set; } = string.Empty;

    public int StatusId { get; set; }

    public string? Cen { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public string? InventoryDocumentCen { get; set; }

    public virtual Supplier? Supplier { get; set; }

    public virtual PurchaseStatus? Status { get; set; }

    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
