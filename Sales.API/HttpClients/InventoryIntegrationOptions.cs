namespace Sales.API.HttpClients;

public class InventoryIntegrationOptions
{
    // Company and warehouse are no longer hardcoded here: companyCen comes from the
    // route, and warehouseCen is selected on the frontend and persisted on each ticket.
    public string Source { get; set; } = "SALES_PAYMENT";
}
