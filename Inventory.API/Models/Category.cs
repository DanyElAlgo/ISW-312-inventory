namespace Inventory.API.Models;

public partial class Category
{
    public int Id { get; set; }

    public int? BusinessId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Cen { get; set; }

    public bool IsActive { get; set; }

    public virtual Business? Business { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
