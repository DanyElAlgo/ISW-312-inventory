using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs.Contract;

public sealed class WarehouseDto
{
    public required string WarehouseCen { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateWarehouseRequest
{
    [Required]
    public required string Name { get; init; }
}

public sealed class UpdateWarehouseRequest
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
}