namespace Inventory.API.Models;

public partial class InventoryDocumentLine
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public int ProductId { get; set; }

    public double Quantity { get; set; }

    public decimal? UnitCost { get; set; }

    public virtual InventoryDocument? Document { get; set; }

    public virtual Product? Product { get; set; }
}
