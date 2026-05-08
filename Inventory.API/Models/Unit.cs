using System;
using System.Collections.Generic;

namespace Inventory.API.Models;

public partial class Unit
{
    public int Id { get; set; }

    public int? BusinessId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Abbreviation { get; set; }

    public string? Cen { get; set; }

    public bool IsActive { get; set; }

    public virtual Business? Business { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
