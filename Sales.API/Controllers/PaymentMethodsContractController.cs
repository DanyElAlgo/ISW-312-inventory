using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("PaymentMethodsContract")]
public class PaymentMethodsContractController : ControllerBase
{
    private readonly PaymentMethodsService _paymentMethodsService;

    public PaymentMethodsContractController(PaymentMethodsService paymentMethodsService)
    {
        _paymentMethodsService = paymentMethodsService;
    }

    /// <summary>Lista metodos de pago</summary>
    [HttpGet("api/sales/payment-methods")]
    [ProducesResponseType(typeof(List<PaymentMethodContractResponse>), 200)]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var methods = await _paymentMethodsService.GetPaymentMethodsAsync();
        return Ok(methods);
    }
}
