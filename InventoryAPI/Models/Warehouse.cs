using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Warehouse
{
    public int Warehouseid { get; set; }

    public int Businessid { get; set; }

    public string Warehousename { get; set; } = null!;

    public virtual Business Business { get; set; } = null!;

    public virtual ICollection<Kardex> KardexWarehouseprimaries { get; set; } = new List<Kardex>();

    public virtual ICollection<Kardex> KardexWarehousesecondaries { get; set; } = new List<Kardex>();
}
