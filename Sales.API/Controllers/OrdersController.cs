using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly SalesCrudService _service;

    public OrdersController(SalesCrudService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderGetDto>>> GetAll()
    {
        return Ok(await _service.GetOrdersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderGetDto>> GetById(int id)
    {
        var order = await _service.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new { message = "Order not found." });
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderGetDto>> Create([FromBody] OrderCreateDto dto)
    {
        var order = await _service.CreateOrderAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderGetDto>> Update(int id, [FromBody] OrderUpdateDto dto)
    {
        var order = await _service.UpdateOrderAsync(id, dto);
        if (order == null)
            return NotFound(new { message = "Order not found." });
        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteOrderAsync(id);
        if (!deleted)
            return NotFound(new { message = "Order not found." });
        return NoContent();
    }

    [HttpGet("{id}/items")]
    public async Task<ActionResult<IEnumerable<OrderItemGetDto>>> GetItems(int id)
    {
        return Ok(await _service.GetOrderItemsAsync(id));
    }

    [HttpGet("{orderId}/payments")]
    public async Task<ActionResult<IEnumerable<PaymentGetDto>>> GetPayments(int orderId)
    {
        return Ok(await _service.GetPaymentsAsync(orderId));
    }
}
