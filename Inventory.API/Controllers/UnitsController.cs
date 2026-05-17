using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class UnitsController : ControllerBase
{
    private readonly UnitsService _service;

    public UnitsController(UnitsService service)
    {
        _service = service;
    }

    [HttpGet("companies/{companyCen}/units")]
    public async Task<ActionResult<IReadOnlyList<UnitDto>>> GetUnits(string companyCen)
    {
        var units = await _service.GetUnitsAsync(companyCen);
        if (units == null)
            return NotFound(new { message = "Company not found." });

        return Ok(units);
    }

    [HttpPost("companies/{companyCen}/units")]
    public async Task<ActionResult<UnitDto>> CreateUnit(string companyCen, [FromBody] CreateUnitRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var unit = await _service.CreateUnitAsync(companyCen, dto);
            if (unit == null)
                return NotFound(new { message = "Company not found." });

            return Ok(unit);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/units/{unitCen}")]
    public async Task<ActionResult<UnitDto>> UpdateUnit(string companyCen, string unitCen, [FromBody] UpdateUnitRequest dto)
    {
        try
        {
            var unit = await _service.UpdateUnitAsync(companyCen, unitCen, dto);
            if (unit == null)
                return NotFound(new { message = "Unit not found." });

            return Ok(unit);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
