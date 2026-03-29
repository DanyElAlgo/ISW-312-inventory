using Inventory.API.DTOs;
using Inventory.API.Repositories;

namespace Inventory.API.Services;

public class ProductSearchService
{
    private readonly ProductSearchRepository _repository;

    public ProductSearchService(ProductSearchRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedProductSearchDto> SearchProductsAsync(ProductSearchFilterDto filter)
    {
        var (items, totalCount) = await _repository.SearchAsync(
            searchTerm: filter.SearchTerm,
            categoryId: filter.CategoryId,
            statusId: filter.StatusId,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize);

        return new PaginatedProductSearchDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }
}
