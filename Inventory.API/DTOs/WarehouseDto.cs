namespace Inventory.API.DTOs;

public class WarehouseGetDto
{
    public int Id { get; set; }
    public int? BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public string? Name { get; set; }
    public int? ProductCount { get; set; }
}

public class WarehouseCreateDto
{
    public required int BusinessId { get; set; }
    public required string Name { get; set; }
}

public class WarehouseUpdateDto
{
    public int? BusinessId { get; set; }
    public string? Name { get; set; }
}
