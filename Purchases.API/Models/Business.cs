namespace Purchases.API.Models;

// Read-only reference to inventory.business so Purchases can resolve companyCen -> business_id
// without crossing service boundaries for every lookup. Purchases never writes to this table.
public partial class Business
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Cen { get; set; }

    public bool IsActive { get; set; }
}
