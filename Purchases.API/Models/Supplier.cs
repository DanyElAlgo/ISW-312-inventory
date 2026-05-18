namespace Purchases.API.Models;

public partial class Supplier
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Cen { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
