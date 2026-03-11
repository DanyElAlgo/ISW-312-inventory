using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class WarehouseProduct
{
    public int Id { get; set; }

    public int? WarehouseId { get; set; }

    public int? ProductId { get; set; }

    public int? StatusId { get; set; }

    public int? StockLeft { get; set; }

    public int? LowStockQty { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ProductStatus? Status { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
