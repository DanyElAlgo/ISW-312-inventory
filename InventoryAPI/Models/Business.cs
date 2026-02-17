using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Business
{
    public int Businessid { get; set; }

    public string Businessname { get; set; } = null!;

    public virtual ICollection<Kardex> Kardices { get; set; } = new List<Kardex>();

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
