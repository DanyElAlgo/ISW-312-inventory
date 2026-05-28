using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class WarehousesController : ControllerBase
{
    private readonly WarehousesService _service;

    public WarehousesController(WarehousesService service)
    {
        _service = service;
    }

    [HttpGet("companies/{companyCen}/warehouses")]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetWarehouses(string companyCen)
    {
        var warehouses = await _service.GetWarehousesAsync(companyCen);
        if (warehouses == null)
            return NotFound(new { message = "Company not found." });

        return Ok(warehouses);
    }
}
