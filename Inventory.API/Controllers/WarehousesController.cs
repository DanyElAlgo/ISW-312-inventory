using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class WarehousesController : ControllerBase
{
    private readonly InventoryContractService _service;

    public WarehousesController(InventoryContractService service)
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

    [HttpPost("companies/{companyCen}/warehouses")]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse(string companyCen, [FromBody] CreateWarehouseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var warehouse = await _service.CreateWarehouseAsync(companyCen, dto);
            if (warehouse == null)
                return NotFound(new { message = "Company not found." });

            return Ok(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/warehouses/{warehouseCen}")]
    public async Task<ActionResult<WarehouseDto>> UpdateWarehouse(string companyCen, string warehouseCen, [FromBody] UpdateWarehouseRequest dto)
    {
        try
        {
            var warehouse = await _service.UpdateWarehouseAsync(companyCen, warehouseCen, dto);
            if (warehouse == null)
                return NotFound(new { message = "Warehouse not found." });

            return Ok(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
