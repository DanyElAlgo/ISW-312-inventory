using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Warehouseproduct
{
    public int Warehouseid { get; set; }

    public int Businessid { get; set; }

    public int Productid { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
