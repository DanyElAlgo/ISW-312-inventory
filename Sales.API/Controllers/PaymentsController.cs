using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly SalesCrudService _service;

    public PaymentsController(SalesCrudService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentGetDto>> Create([FromBody] PaymentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var payment = await _service.CreatePaymentAsync(dto);
        return Ok(payment);
    }
}
