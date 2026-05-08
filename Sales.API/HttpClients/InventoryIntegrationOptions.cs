namespace Sales.API.HttpClients;

public class InventoryIntegrationOptions
{
    public string CompanyCen { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public string Source { get; set; } = "SALES_PAYMENT";
}
