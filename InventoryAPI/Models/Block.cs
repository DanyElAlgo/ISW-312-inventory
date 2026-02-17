using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Block
{
    public int Blockid { get; set; }

    public int Hallwayid { get; set; }

    public string Blockname { get; set; } = null!;

    public virtual Hallway Hallway { get; set; } = null!;
}
