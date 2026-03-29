namespace Sales.API.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public double? Qty { get; set; }

    public string? AdditionalNote { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? StatusId { get; set; }

    public virtual ICollection<CommandItem> CommandItems { get; set; } = new List<CommandItem>();

    public virtual OrderTicket? Order { get; set; }

    public virtual OrderStatus? Status { get; set; }
}
