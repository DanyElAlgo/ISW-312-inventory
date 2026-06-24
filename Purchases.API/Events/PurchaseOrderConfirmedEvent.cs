namespace Purchases.API.Events;

public sealed record PurchaseOrderConfirmedEvent
{
    public required string CompanyCen { get; init; }
    public required string OrderCen { get; init; }
    public string? SupplierCen { get; init; }
    public required string WarehouseCen { get; init; }
    public DateTime ConfirmedAt { get; init; }
    public required IReadOnlyList<PurchaseOrderConfirmedItem> Items { get; init; }
}

public sealed record PurchaseOrderConfirmedItem
{
    public required string ProductCen { get; init; }
    public string? ProductName { get; init; }
    public int Quantity { get; init; }
}
