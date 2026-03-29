namespace Inventory.API.DTOs;

public class ProductGetDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool IsActive { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public double? UnitQty { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class ProductActivationDto
{
    public bool IsActive { get; set; }
}
