namespace Sales.API.Models;

/// <summary>
/// Sales-owned mapping of a company to the warehouse used when a ticket is created
/// without an explicit warehouseCen. Keyed by company CEN (no cross-schema FK — the
/// inventory business/warehouse tables live in another schema and are reached via the API).
/// </summary>
public partial class DefaultWarehouse
{
    public string CompanyCen { get; set; } = string.Empty;

    public string WarehouseCen { get; set; } = string.Empty;
}
