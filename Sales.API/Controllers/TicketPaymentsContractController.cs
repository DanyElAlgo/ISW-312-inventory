using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("TicketPaymentsContract")]
public class TicketPaymentsContractController : ControllerBase
{
    private readonly PosService _posService;

    public TicketPaymentsContractController(PosService posService)
    {
        _posService = posService;
    }

    /// <summary>Procesa el pago de un ticket</summary>
    /// <remarks>Registra el pago de un ticket usando el metodo indicado. Usar cuando el cliente finaliza la compra.</remarks>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/payment")]
    [ProducesResponseType(typeof(PayTicketContractResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProcessRestaurantOrderPaymentResultDto), 409)]
    public async Task<IActionResult> PayTicket(
        string companyCen,
        string ticketCen,
        [FromBody] PayTicketContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentMethodCode))
            return BadRequest("paymentMethodCode is required.");

        try
        {
            var (success, conflict) = await _posService.PayTicketContractAsync(companyCen, ticketCen, request.PaymentMethodCode);

            if (conflict != null)
                return Conflict(conflict);

            if (success == null)
                return NotFound();

            return Ok(success);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
