using Microsoft.AspNetCore.Mvc;
using InventoryAPI.DTOs;
using InventoryAPI.Services;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    private readonly ProductSearchService _searchService;

    public ProductsController(ProductService service, ProductSearchService searchService)
    {
        _service = service;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductGetDto>>> GetAllProducts()
    {
        var products = await _service.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductGetDto>> GetProductById(int id)
    {
        var product = await _service.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductGetDto>> CreateProduct([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _service.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductGetDto>> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _service.UpdateProductAsync(id, dto);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var success = await _service.DeleteProductAsync(id);
        if (!success)
            return NotFound(new { message = "Product not found" });

        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<KardexGetDto>>> GetProductHistory(int id)
    {
        var history = await _service.GetProductHistoryAsync(id);
        return Ok(history);
    }

    /// <param name="filter">Search filter with optional searchTerm, categoryId, statusId and pagination</param>
    [HttpPost("search")]
    public async Task<ActionResult<PaginatedProductSearchDto>> SearchProducts([FromBody] ProductSearchFilterDto filter)
    {
        // Validate pagination parameters
        if (filter.PageNumber < 1)
            filter.PageNumber = 1;
        if (filter.PageSize < 1 || filter.PageSize > 100)
            filter.PageSize = 10;

        var result = await _searchService.SearchProductsAsync(filter);
        return Ok(result);
    }
}
