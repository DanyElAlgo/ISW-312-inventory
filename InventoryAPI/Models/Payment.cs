using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? PaymentTypeId { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual OrderTicket? Order { get; set; }

    public virtual PaymentType? PaymentType { get; set; }
}
