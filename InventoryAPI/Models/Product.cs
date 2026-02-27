using System;
using System.Collections.Generic;

namespace InventoryAPI.Models;

public partial class Product
{
    public int Productid { get; set; }

    public string Name { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Metricunit { get; set; } = null!;

    public bool Status { get; set; }

    public int? Batch { get; set; }

    public int Stockleft { get; set; }

    public int? Lowstockqty { get; set; }

    public virtual ICollection<Kardex> Kardices { get; set; } = new List<Kardex>();
}
