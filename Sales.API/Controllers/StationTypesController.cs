using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationTypesController : ControllerBase
{
    private readonly StationManagementService _service;

    public StationTypesController(StationManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StationTypeDto>>> GetAll()
    {
        var result = await _service.GetAllStationTypesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StationTypeDto>> GetById(int id)
    {
        var result = await _service.GetStationTypeByIdAsync(id);
        if (result == null)
            return NotFound(new { message = "Station type not found." });

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<StationTypeDto>> Create([FromBody] StationTypeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateStationTypeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StationTypeDto>> Update(int id, [FromBody] StationTypeUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.UpdateStationTypeAsync(id, dto);
        if (result == null)
            return NotFound(new { message = "Station type not found." });

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteStationTypeAsync(id);
            if (!success)
                return NotFound(new { message = "Station type not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/coverage")]
    public async Task<ActionResult<IEnumerable<int>>> GetCoverage(int id)
    {
        var categoryIds = await _service.GetCoverageForTypeAsync(id);
        return Ok(categoryIds);
    }

    [HttpPut("{id}/coverage")]
    public async Task<IActionResult> SetCoverage(int id, [FromBody] CoverageAssignDto dto)
    {
        try
        {
            await _service.SetCoverageForTypeAsync(id, dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
