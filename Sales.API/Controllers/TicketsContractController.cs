using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("TicketsContract")]
public class TicketsContractController : ControllerBase
{
    private readonly PosService _posService;

    public TicketsContractController(PosService posService)
    {
        _posService = posService;
    }

    /// <summary>Lista tickets del dia</summary>
    /// <remarks>Devuelve los tickets activos del dia actual. Usar para paneles de operacion o historico corto.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/tickets")]
    [ProducesResponseType(typeof(List<TicketContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTickets(string companyCen)
    {
        var result = await _posService.GetTicketsAsync(companyCen);
        return Ok(result);
    }

    /// <summary>Crea un ticket</summary>
    /// <remarks>Abre un ticket para una nueva orden en la empresa. Usar al iniciar una atencion de mesa o pedido.</remarks>
    [HttpPost("api/sales/companies/{companyCen}/tickets")]
    [ProducesResponseType(typeof(TicketContractResponse), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateTicket(
        string companyCen,
        [FromBody] CreateTicketContractRequest request)
    {
        var result = await _posService.CreateTicketContractAsync(companyCen, request);
        if (result == null)
            return NotFound();

        return CreatedAtAction(nameof(GetTickets), new { companyCen }, result);
    }

    /// <summary>Lista items de un ticket</summary>
    /// <remarks>
    /// Devuelve los items asociados a un ticket.
    /// Usar para ver detalle de productos y cantidades.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpGet("api/sales/companies/{companyCen}/tickets/{ticketCen}/items")]
    [ProducesResponseType(typeof(List<TicketItemContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTicketItems(string companyCen, string ticketCen)
    {
        var result = await _posService.GetTicketItemsContractAsync(ticketCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Agrega un item a un ticket</summary>
    /// <remarks>
    /// Crea un nuevo item dentro del ticket con producto y cantidad.
    /// Usar para registrar pedidos de clientes.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/items")]
    [ProducesResponseType(typeof(TicketItemContractResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddTicketItem(
        string companyCen,
        string ticketCen,
        [FromBody] CreateTicketItemContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductCen))
            return BadRequest("productCen is required.");

        if (request.Quantity <= 0)
            return BadRequest("quantity must be greater than 0.");

        try
        {
            var result = await _posService.AddTicketItemContractAsync(ticketCen, request);
            if (result == null)
                return NotFound();
            return CreatedAtAction(nameof(GetTicketItems), new { companyCen, ticketCen }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Actualiza un item de ticket</summary>
    /// <remarks>
    /// Modifica cantidad o nota del item en el ticket.
    /// Usar para ajustes solicitados por el cliente.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpPatch("api/sales/companies/{companyCen}/tickets/{ticketCen}/items/{ticketItemCen}")]
    [ProducesResponseType(typeof(TicketItemContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTicketItem(
        string companyCen,
        string ticketCen,
        string ticketItemCen,
        [FromBody] UpdateTicketItemContractRequest request)
    {
        var result = await _posService.UpdateTicketItemContractAsync(ticketCen, ticketItemCen, request);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Reenvia un item a cocina</summary>
    /// <remarks>
    /// Marca un item para reenvio en el flujo de cocina.
    /// Usar cuando un item debe prepararse nuevamente.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/items/{ticketItemCen}/resend")]
    [ProducesResponseType(typeof(TicketItemContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResendTicketItem(
        string companyCen,
        string ticketCen,
        string ticketItemCen)
    {
        var result = await _posService.ResendTicketItemAsync(ticketCen, ticketItemCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Envia un ticket a cocina</summary>
    /// <remarks>
    /// Cambia el estado del ticket para iniciar preparacion.
    /// Usar cuando el pedido esta listo para cocina.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/send")]
    [ProducesResponseType(typeof(List<TicketItemContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendTicketToKitchen(string companyCen, string ticketCen)
    {
        try
        {
            var result = await _posService.SendTicketToKitchenContractAsync(companyCen, ticketCen);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Asigna mesero a un ticket</summary>
    /// <remarks>Asocia un mesero al ticket abierto. Usar cuando se reasigna la atencion de la mesa.</remarks>
    [HttpPut("api/sales/companies/{companyCen}/tickets/{ticketCen}/waiter")]
    [ProducesResponseType(typeof(AssignTicketWaiterContractResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignWaiter(
        string companyCen,
        string ticketCen,
        [FromBody] AssignTicketWaiterContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WaiterCen))
            return BadRequest("waiterCen is required.");

        try
        {
            var result = await _posService.AssignTicketWaiterContractAsync(ticketCen, request);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Cancela un ticket</summary>
    /// <remarks>Cancela un ticket activo por solicitud del cliente o error. Usar antes del pago si el pedido no debe continuar.</remarks>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/cancel")]
    [ProducesResponseType(typeof(CancelTicketContractResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CancelTicket(
        string companyCen,
        string ticketCen,
        [FromBody] CancelTicketContractRequest? request)
    {
        try
        {
            var result = await _posService.CancelTicketContractAsync(ticketCen, request?.Reason);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("paid") || ex.Message.Contains("already cancelled"))
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Imprime un ticket</summary>
    /// <remarks>Genera el PDF del ticket para impresion o envio. Usar al cerrar la cuenta o para comprobantes.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/tickets/{ticketCen}/print")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> PrintTicket(string companyCen, string ticketCen)
    {
        try
        {
            var bytes = await _posService.PrintTicketAsync(ticketCen);
            return File(bytes, "text/html", $"ticket-{ticketCen}.html");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("Invalid"))
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Obtiene totales de un ticket</summary>
    /// <remarks>Devuelve subtotal, impuesto y total del ticket. Usar para mostrar resumen antes de cobrar.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/tickets/{ticketCen}/totals")]
    [ProducesResponseType(typeof(TicketTotalsContractResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> GetTicketTotals(string companyCen, string ticketCen)
    {
        var result = await _posService.GetTicketTotalsAsync(ticketCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }
}
