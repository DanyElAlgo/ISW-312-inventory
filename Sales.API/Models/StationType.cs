namespace Sales.API.Models;

public partial class StationType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();
    
    public ICollection<int> CategoryIds { get; set; } = new List<int>();
}
