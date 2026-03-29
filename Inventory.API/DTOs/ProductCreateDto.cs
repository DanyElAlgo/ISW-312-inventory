using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs;

public class ProductCreateDto
{
    [Required]
    public required string Name { get; set; }
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int UnitId { get; set; }

    public double? UnitQty { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public decimal Price { get; set; }
}
