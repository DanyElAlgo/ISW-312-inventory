using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/paymenttypes")]
public class PaymentTypesController : ControllerBase
{
    private readonly SalesCrudService _service;

    public PaymentTypesController(SalesCrudService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentTypeGetDto>>> GetAll()
    {
        return Ok(await _service.GetPaymentTypesAsync());
    }
}
