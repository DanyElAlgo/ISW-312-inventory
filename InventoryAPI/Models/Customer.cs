using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}
