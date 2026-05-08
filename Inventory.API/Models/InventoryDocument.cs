namespace Inventory.API.Models;

public partial class InventoryDocument
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public int WarehouseId { get; set; }

    public string DocumentCen { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string? ExternalReference { get; set; }

    public string? Source { get; set; }

    public string? ReferenceCen { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Business? Business { get; set; }

    public virtual Warehouse? Warehouse { get; set; }

    public virtual ICollection<InventoryDocumentLine> Lines { get; set; } = new List<InventoryDocumentLine>();

    public virtual ICollection<Kardex> KardexEntries { get; set; } = new List<Kardex>();
}
