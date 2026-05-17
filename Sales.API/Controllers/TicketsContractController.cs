using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("TicketsContract")]
public class TicketsContractController : ControllerBase
{
    private readonly OrderTicketsService _ticketsService;
    private readonly OrderItemsService _itemsService;
    private readonly KdsService _kdsService;

    public TicketsContractController(
        OrderTicketsService ticketsService,
        OrderItemsService itemsService,
        KdsService kdsService)
    {
        _ticketsService = ticketsService;
        _itemsService = itemsService;
        _kdsService = kdsService;
    }

    /// <summary>Lista tickets del dia</summary>
    [HttpGet("api/sales/companies/{companyCen}/tickets")]
    [ProducesResponseType(typeof(List<TicketContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTickets(string companyCen)
    {
        var result = await _ticketsService.GetTicketsAsync(companyCen);
        return Ok(result);
    }

    /// <summary>Crea un ticket</summary>
    [HttpPost("api/sales/companies/{companyCen}/tickets")]
    [ProducesResponseType(typeof(TicketContractResponse), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateTicket(
        string companyCen,
        [FromBody] CreateTicketContractRequest request)
    {
        var result = await _ticketsService.CreateTicketAsync(companyCen, request);
        if (result == null)
            return NotFound();

        return CreatedAtAction(nameof(GetTickets), new { companyCen }, result);
    }

    /// <summary>Lista items de un ticket</summary>
    [HttpGet("api/sales/companies/{companyCen}/tickets/{ticketCen}/items")]
    [ProducesResponseType(typeof(List<TicketItemContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTicketItems(string companyCen, string ticketCen)
    {
        var result = await _itemsService.GetItemsAsync(ticketCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Agrega un item a un ticket</summary>
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
            var result = await _itemsService.AddItemAsync(ticketCen, request);
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
    [HttpPatch("api/sales/companies/{companyCen}/tickets/{ticketCen}/items/{ticketItemCen}")]
    [ProducesResponseType(typeof(TicketItemContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTicketItem(
        string companyCen,
        string ticketCen,
        string ticketItemCen,
        [FromBody] UpdateTicketItemContractRequest request)
    {
        var result = await _itemsService.UpdateItemAsync(ticketCen, ticketItemCen, request);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Reenvia un item a cocina</summary>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/items/{ticketItemCen}/resend")]
    [ProducesResponseType(typeof(TicketItemContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResendTicketItem(
        string companyCen,
        string ticketCen,
        string ticketItemCen)
    {
        var result = await _itemsService.ResendItemAsync(ticketCen, ticketItemCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Envia un ticket a cocina</summary>
    [HttpPost("api/sales/companies/{companyCen}/tickets/{ticketCen}/send")]
    [ProducesResponseType(typeof(List<TicketItemContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendTicketToKitchen(string companyCen, string ticketCen)
    {
        try
        {
            var result = await _ticketsService.SendTicketToKitchenAsync(
                ticketCen,
                _kdsService.ResolveStationForProductAsync);
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
            var result = await _ticketsService.AssignWaiterAsync(ticketCen, request);
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
            var result = await _ticketsService.CancelTicketAsync(ticketCen, request?.Reason);
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
    [HttpGet("api/sales/companies/{companyCen}/tickets/{ticketCen}/print")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PrintTicket(string companyCen, string ticketCen)
    {
        try
        {
            var bytes = await _ticketsService.PrintTicketAsync(ticketCen);
            return File(bytes, "text/html", $"ticket-{ticketCen}.html");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("Invalid"))
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Obtiene totales de un ticket</summary>
    [HttpGet("api/sales/companies/{companyCen}/tickets/{ticketCen}/totals")]
    [ProducesResponseType(typeof(TicketTotalsContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTicketTotals(string companyCen, string ticketCen)
    {
        var result = await _ticketsService.GetTicketTotalsAsync(ticketCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }
}
