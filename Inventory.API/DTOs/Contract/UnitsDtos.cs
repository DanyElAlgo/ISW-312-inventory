using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs.Contract;

public sealed class UnitDto
{
    public required string UnitCen { get; init; }
    public required string Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateUnitRequest
{
    [Required]
    public required string Name { get; init; }

    public string? Abbreviation { get; init; }
}

public sealed class UpdateUnitRequest
{
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public bool? IsActive { get; init; }
}