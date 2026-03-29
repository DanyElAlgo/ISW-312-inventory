using System;

namespace Sales.API.Models;

public partial class OrderStatus
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderTicket> OrderTickets { get; set; } = new List<OrderTicket>();
}
