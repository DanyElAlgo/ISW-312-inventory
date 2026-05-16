using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.HttpClients;
using Microsoft.Extensions.Options;

namespace Sales.API.Controllers;

[ApiController]
[Tags("CatalogContract")]
public class CatalogContractController : ControllerBase
{
    private readonly InventoryClient _inventoryClient;
    private readonly InventoryIntegrationOptions _options;

    public CatalogContractController(
        InventoryClient inventoryClient,
        IOptions<InventoryIntegrationOptions> options)
    {
        _inventoryClient = inventoryClient;
        _options = options.Value;
    }

    /// <summary>Lista productos vendibles para ventas</summary>
    /// <remarks>
    /// Devuelve productos disponibles para venta en la empresa indicada.
    /// Usar para catalogos POS y seleccion de items al crear tickets.
    /// Integra con el API de Inventario para obtener productos vendibles.
    /// </remarks>
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

        var products = await _inventoryClient.GetProductsAsync(
            companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize);

        if (products == null)
            return NotFound();

        var result = products.Select(p => new SellableProductContractDto
        {
            ProductCen = p.ProductCen,
            Name = p.Name,
            CategoryCen = p.CategoryCen ?? string.Empty,
            CategoryName = p.CategoryName ?? string.Empty,
            SalePrice = p.SalePrice,
            AvailableQuantity = (double)p.AvailableQuantity,
            IsAvailable = p.Status.ToUpperInvariant() is not ("INACTIVE" or "OUT_OF_STOCK"),
            StationCode = p.StationCode
        }).ToList();

        return Ok(result);
    }
}
