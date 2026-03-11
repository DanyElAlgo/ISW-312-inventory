using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class StationType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
