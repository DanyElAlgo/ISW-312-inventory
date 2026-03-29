using System;

namespace Sales.API.Models;

public partial class Station
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? TypeId { get; set; }

    public virtual ICollection<CommandItem> CommandItems { get; set; } = new List<CommandItem>();

    public virtual StationType? Type { get; set; }
}
