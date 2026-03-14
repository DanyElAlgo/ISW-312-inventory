namespace InventoryAPI.DTOs;

public class UnitGetDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class UnitCreateDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UnitUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
