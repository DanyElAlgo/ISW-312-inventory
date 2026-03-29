using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly StationManagementService _service;

    public StationsController(StationManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StationDto>>> GetAll()
    {
        var result = await _service.GetAllStationsAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<StationDto>> Create([FromBody] StationCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateStationAsync(dto);
            return CreatedAtAction(nameof(GetAll), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteStationAsync(id);
        if (!success)
            return NotFound(new { message = "Station not found." });

        return NoContent();
    }
}
