using System;
using System.Collections.Generic;

namespace Inventory.API.Models;

public partial class Kardex
{
    public int Id { get; set; }

    public int? WarehouseId { get; set; }

    public int? ProductId { get; set; }

    public int? DocumentId { get; set; }

    public string? MovementCen { get; set; }

    public string? ActionType { get; set; }

    public double? ActionQty { get; set; }

    public DateTime? TimeStamp { get; set; }

    public string? Reason { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Warehouse? Warehouse { get; set; }

    public virtual InventoryDocument? Document { get; set; }
}
