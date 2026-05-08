namespace Inventory.API.Models;

public partial class Product
{
    public int Id { get; set; }

    public int? BusinessId { get; set; }

    public string? Cen { get; set; }

    public string? Sku { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public decimal? CostPrice { get; set; }

    public bool? IsActive { get; set; }

    public int? UnitId { get; set; }

    public double? UnitQty { get; set; }

    public int? CategoryId { get; set; }

    public string? StationCode { get; set; }

    public int ReorderLevel { get; set; }

    public virtual Business? Business { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Kardex> Kardices { get; set; } = new List<Kardex>();

    public virtual Unit? Unit { get; set; }

    public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();
}
