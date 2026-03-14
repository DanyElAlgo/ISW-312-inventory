using Microsoft.AspNetCore.Mvc;
using InventoryAPI.DTOs;
using InventoryAPI.Services;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly UnitService _service;

    public UnitsController(UnitService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UnitGetDto>>> GetAll()
    {
        var units = await _service.GetAllUnitsAsync();
        return Ok(units);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UnitGetDto>> GetById(int id)
    {
        var unit = await _service.GetUnitByIdAsync(id);
        if (unit == null)
            return NotFound(new { message = "Unit not found" });

        return Ok(unit);
    }

    [HttpPost]
    public async Task<ActionResult<UnitGetDto>> Create([FromBody] UnitCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var unit = await _service.CreateUnitAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UnitGetDto>> Update(int id, [FromBody] UnitUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var unit = await _service.UpdateUnitAsync(id, dto);
        if (unit == null)
            return NotFound(new { message = "Unit not found" });

        return Ok(unit);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteUnitAsync(id);
        if (!success)
            return NotFound(new { message = "Unit not found" });

        return NoContent();
    }
}
