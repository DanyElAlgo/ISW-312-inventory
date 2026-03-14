namespace InventoryAPI.DTOs;

public class ProductGetDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public double? UnitQty { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
