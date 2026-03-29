namespace Shared.Contracts.DTOs;

public class ProductReferenceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int? CategoryId { get; set; }
    public bool HasAvailableStock { get; set; }
}
