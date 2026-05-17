using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("CatalogContract")]
public class CatalogContractController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public CatalogContractController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>Lista productos vendibles para ventas</summary>
    [HttpGet("api/sales/companies/{companyCen}/catalog/products")]
    [ProducesResponseType(typeof(List<SellableProductContractDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCatalogProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("page and pageSize must be >= 1.");

        var products = await _catalogService.GetSellableProductsAsync(
            companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize);

        if (products == null)
            return NotFound();

        return Ok(products);
    }
}
