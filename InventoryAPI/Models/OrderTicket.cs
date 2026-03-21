using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class OrderTicket
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? StatusId { get; set; }

    public decimal? TaxRateSnapshot { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderCommand> OrderCommands { get; set; } = new List<OrderCommand>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual OrderStatus? Status { get; set; }
}
