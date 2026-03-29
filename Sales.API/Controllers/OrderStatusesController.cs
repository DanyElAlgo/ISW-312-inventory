using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/orderstatuses")]
public class OrderStatusesController : ControllerBase
{
    private readonly SalesCrudService _service;

    public OrderStatusesController(SalesCrudService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderStatusGetDto>>> GetAll()
    {
        return Ok(await _service.GetOrderStatusesAsync());
    }
}
