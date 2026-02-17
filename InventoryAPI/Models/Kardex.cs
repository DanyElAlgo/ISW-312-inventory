using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Kardex
{
    public int Kardexid { get; set; }

    public int Warehouseprimaryid { get; set; }

    public int? Warehousesecondaryid { get; set; }

    public int Businessid { get; set; }

    public int Productid { get; set; }

    public string Actiontype { get; set; } = null!;

    public int Actionqty { get; set; }

    public string? Reason { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Business Business { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouseprimary { get; set; } = null!;

    public virtual Warehouse? Warehousesecondary { get; set; }
}
