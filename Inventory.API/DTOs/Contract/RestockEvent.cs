namespace Inventory.API.DTOs.Contract;

public sealed class RestockEvent
{
    public required string CompanyCen { get; init; }
    public required string ProductCen { get; init; }
    public required string ProductName { get; init; }
    public decimal Quantity { get; init; }
    public required string WarehouseCen { get; init; }
    public DateTime OccurredAt { get; init; }
}
