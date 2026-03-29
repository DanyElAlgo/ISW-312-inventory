using Microsoft.AspNetCore.Mvc;
using Inventory.API.DTOs;
using Inventory.API.Services;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly BusinessService _service;

    public BusinessesController(BusinessService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessGetDto>>> GetAll()
    {
        var businesses = await _service.GetAllBusinessesAsync();
        return Ok(businesses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BusinessGetDto>> GetById(int id)
    {
        var business = await _service.GetBusinessByIdAsync(id);
        if (business == null)
            return NotFound(new { message = "Business not found" });

        return Ok(business);
    }

    [HttpPost]
    public async Task<ActionResult<BusinessGetDto>> Create([FromBody] BusinessCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var business = await _service.CreateBusinessAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = business.Id }, business);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BusinessGetDto>> Update(int id, [FromBody] BusinessUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var business = await _service.UpdateBusinessAsync(id, dto);
        if (business == null)
            return NotFound(new { message = "Business not found" });

        return Ok(business);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteBusinessAsync(id);
        if (!success)
            return NotFound(new { message = "Business not found" });

        return NoContent();
    }
}
