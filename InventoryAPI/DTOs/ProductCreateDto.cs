namespace InventoryAPI.DTOs;

public class ProductCreateDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? UnitId { get; set; }
    public double? UnitQty { get; set; }
    public int? CategoryId { get; set; }
}
