using System;
using System.Collections.Generic;

namespace Inventory.API.Models;

public partial class Warehouse
{
    public int Id { get; set; }

    public int? BusinessId { get; set; }

    public string? Name { get; set; }

    public string? Cen { get; set; }

    public bool IsActive { get; set; }

    public virtual Business? Business { get; set; }

    public virtual ICollection<Kardex> Kardices { get; set; } = new List<Kardex>();

    public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();
}
