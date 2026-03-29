using System;
using System.Collections.Generic;

namespace Inventory.API.Models;

public partial class Business
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
