using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderItemsController : ControllerBase
{
    private readonly SalesCrudService _service;

    public OrderItemsController(SalesCrudService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<OrderItemGetDto>> Create([FromBody] OrderItemCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = await _service.CreateOrderItemAsync(dto);
        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderItemGetDto>> Update(int id, [FromBody] OrderItemUpdateDto dto)
    {
        var item = await _service.UpdateOrderItemAsync(id, dto);
        if (item == null)
            return NotFound(new { message = "Order item not found." });
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteOrderItemAsync(id);
        if (!deleted)
            return NotFound(new { message = "Order item not found." });
        return NoContent();
    }
}
