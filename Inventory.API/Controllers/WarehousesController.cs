using Microsoft.AspNetCore.Mvc;
using Inventory.API.DTOs;
using Inventory.API.Services;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly WarehouseService _service;

    public WarehousesController(WarehouseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseGetDto>>> GetAll()
    {
        var warehouses = await _service.GetAllWarehousesAsync();
        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseGetDto>> GetById(int id)
    {
        var warehouse = await _service.GetWarehouseByIdAsync(id);
        if (warehouse == null)
            return NotFound(new { message = "Warehouse not found" });

        return Ok(warehouse);
    }

    [HttpGet("business/{businessId}")]
    public async Task<ActionResult<IEnumerable<WarehouseGetDto>>> GetByBusinessId(int businessId)
    {
        var warehouses = await _service.GetWarehousesByBusinessIdAsync(businessId);
        return Ok(warehouses);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseGetDto>> Create([FromBody] WarehouseCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var warehouse = await _service.CreateWarehouseAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WarehouseGetDto>> Update(int id, [FromBody] WarehouseUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var warehouse = await _service.UpdateWarehouseAsync(id, dto);
        if (warehouse == null)
            return NotFound(new { message = "Warehouse not found" });

        return Ok(warehouse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteWarehouseAsync(id);
        if (!success)
            return NotFound(new { message = "Warehouse not found" });

        return NoContent();
    }
}
