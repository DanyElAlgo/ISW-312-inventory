namespace Sales.API.Models;

public partial class OrderTicket
{
    public int Id { get; set; }

    public string? Cen { get; set; }

    public int? CustomerId { get; set; }

    public int? StatusId { get; set; }

    public decimal? TaxRateSnapshot { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int DailyNumber { get; set; }

    public string? CancellationReason { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderCommand> OrderCommands { get; set; } = new List<OrderCommand>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual OrderStatus? Status { get; set; }
}
