using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _service;

    public ProductsController(ProductsService service)
    {
        _service = service;
    }

    [HttpGet("companies/{companyCen}/products")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? status)
    {
        var products = await _service.GetProductsAsync(companyCen, search, categoryCen, status);
        if (products == null)
            return NotFound(new { message = "Company or category not found." });

        return Ok(products);
    }

    [HttpGet("companies/{companyCen}/products/{productCen}")]
    public async Task<ActionResult<ProductDto>> GetProduct(string companyCen, string productCen)
    {
        var product = await _service.GetProductAsync(companyCen, productCen);
        if (product == null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    [HttpPost("companies/{companyCen}/products")]
    public async Task<ActionResult<ProductDto>> CreateProduct(string companyCen, [FromBody] CreateProductRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await _service.CreateProductAsync(companyCen, dto);
            if (product == null)
                return NotFound(new { message = "Company not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/products/{productCen}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(string companyCen, string productCen, [FromBody] UpdateProductRequest dto)
    {
        try
        {
            var product = await _service.UpdateProductAsync(companyCen, productCen, dto);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies/{companyCen}/sellable-products")]
    public async Task<ActionResult<IReadOnlyList<SellableProductDto>>> GetSellableProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var products = await _service.GetSellableProductsAsync(
            companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize);

        if (products == null)
            return NotFound(new { message = "Company, category, or warehouse not found." });

        return Ok(products);
    }

    [HttpPatch("companies/{companyCen}/products/{productCen}/status")]
    public async Task<ActionResult<ProductDto>> UpdateProductStatus(
        string companyCen,
        string productCen,
        [FromBody] UpdateProductStatusRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await _service.UpdateProductStatusAsync(companyCen, productCen, dto);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
