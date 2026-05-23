namespace Inventory.API.DTOs.Contract;

public sealed class CompanyDto
{
    public required string CompanyCen { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CompanyLookupContractDto
{
    public int CompanyId { get; init; }
    public required string CompanyCen { get; init; }
    public required string Name { get; init; }
}

public sealed class InventoryDashboardDto
{
    public required string CompanyCen { get; init; }
    public int TotalProducts { get; init; }
    public int TotalStockQuantity { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
}