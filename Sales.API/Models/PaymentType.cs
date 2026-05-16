using System;

namespace Sales.API.Models;

public partial class PaymentType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
