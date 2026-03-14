namespace InventoryAPI.DTOs;

public class BusinessGetDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? WarehouseCount { get; set; }
}

public class BusinessCreateDto
{
    public required string Name { get; set; }
}

public class BusinessUpdateDto
{
    public string? Name { get; set; }
}
