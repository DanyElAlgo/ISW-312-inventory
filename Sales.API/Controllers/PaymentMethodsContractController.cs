using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.API.DTOs;
using Sales.API.Models;

namespace Sales.API.Controllers;

[ApiController]
[Tags("PaymentMethodsContract")]
public class PaymentMethodsContractController : ControllerBase
{
    private readonly SalesDbContext _context;

    public PaymentMethodsContractController(SalesDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista metodos de pago</summary>
    /// <remarks>Devuelve los metodos de pago disponibles para ventas. Usar para opciones de pago al procesar tickets.</remarks>
    [HttpGet("api/sales/payment-methods")]
    [ProducesResponseType(typeof(List<PaymentMethodContractResponse>), 200)]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var methods = await _context.PaymentTypes
            .OrderBy(pt => pt.Name)
            .Select(pt => new PaymentMethodContractResponse
            {
                PaymentMethodCode = pt.Code ?? pt.Id.ToString(),
                Name = pt.Name ?? string.Empty,
                IsActive = pt.IsActive
            })
            .ToListAsync();

        return Ok(methods);
    }
}
