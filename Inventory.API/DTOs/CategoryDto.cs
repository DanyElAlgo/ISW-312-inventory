namespace Inventory.API.DTOs;

public class CategoryGetDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ProductCount { get; set; }
}

public class CategoryCreateDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class CategoryUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
