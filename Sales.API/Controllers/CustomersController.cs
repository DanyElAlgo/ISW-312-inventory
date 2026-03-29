using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly SalesCrudService _service;

    public CustomersController(SalesCrudService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerGetDto>>> GetAll()
    {
        return Ok(await _service.GetCustomersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerGetDto>> GetById(int id)
    {
        var customer = await _service.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found." });
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerGetDto>> Create([FromBody] CustomerCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _service.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerGetDto>> Update(int id, [FromBody] CustomerUpdateDto dto)
    {
        var customer = await _service.UpdateCustomerAsync(id, dto);
        if (customer == null)
            return NotFound(new { message = "Customer not found." });
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteCustomerAsync(id);
        if (!deleted)
            return NotFound(new { message = "Customer not found." });
        return NoContent();
    }
}
