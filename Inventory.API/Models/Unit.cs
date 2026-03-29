using System;
using System.Collections.Generic;

namespace Inventory.API.Models;

public partial class Unit
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
