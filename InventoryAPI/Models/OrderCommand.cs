using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class OrderCommand
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? WaiterId { get; set; }

    public virtual OrderTicket? Order { get; set; }

    public virtual Waiter? Waiter { get; set; }
}
