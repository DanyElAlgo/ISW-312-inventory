using System;

namespace Sales.API.Models;

public partial class CommandItem
{
    public int Id { get; set; }

    public int? OrderItemId { get; set; }

    public int? CommandId { get; set; }

    public int? StationId { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual Station? Station { get; set; }
}
