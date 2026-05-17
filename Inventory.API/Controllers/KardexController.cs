using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class KardexController : ControllerBase
{
    private readonly KardexService _service;

    public KardexController(KardexService service)
    {
        _service = service;
    }

    [HttpGet("companies/{companyCen}/products/{productCen}/kardex")]
    public async Task<ActionResult<IReadOnlyList<KardexMovementDto>>> GetKardex(
        string companyCen,
        string productCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var kardex = await _service.GetKardexAsync(companyCen, productCen, warehouseCen, from, to);
        if (kardex == null)
            return NotFound(new { message = "Company, product, or warehouse not found." });

        return Ok(kardex);
    }
}
