using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Hallway
{
    public int Hallwayid { get; set; }

    public int Warehouseid { get; set; }

    public string Hallwayname { get; set; } = null!;

    public virtual ICollection<Block> Blocks { get; set; } = new List<Block>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
