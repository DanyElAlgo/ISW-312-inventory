using System;

namespace Sales.API.Models;

public partial class Waiter
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<OrderCommand> OrderCommands { get; set; } = new List<OrderCommand>();
}
