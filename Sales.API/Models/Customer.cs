using System;

namespace Sales.API.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}
