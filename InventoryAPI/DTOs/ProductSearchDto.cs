namespace InventoryAPI.DTOs;

public class ProductSearchFilterDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? StatusId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ProductSearchResultDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public double? UnitQty { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int TotalStock { get; set; }
    public int LowStockCount { get; set; }
}

public class PaginatedProductSearchDto
{
    public IEnumerable<ProductSearchResultDto> Items { get; set; } = new List<ProductSearchResultDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
