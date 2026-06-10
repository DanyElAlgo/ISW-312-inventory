using Microsoft.AspNetCore.Mvc;
using Purchases.API.DTOs;
using Purchases.API.Services;

namespace Purchases.API.Controllers;

[ApiController]
[Tags("PurchaseOrder")]
public class PurchaseOrdersContractController : ControllerBase
{
    private readonly PurchaseOrdersService _orders;

    public PurchaseOrdersContractController(PurchaseOrdersService orders)
    {
        _orders = orders;
    }

    /// <summary>Lista ordenes de compra</summary>
    [HttpGet("api/purchases/companies/{companyCen}/orders")]
    [ProducesResponseType(typeof(PagedResultDto<PurchaseOrderListDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListOrders(
        string companyCen,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            var result = await _orders.ListAsync(companyCen, status, page, pageSize, sortDescending);
            if (result == null)
                return NotFound(new { message = "Company not found." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Crea una orden de compra</summary>
    [HttpPost("api/purchases/companies/{companyCen}/orders")]
    [ProducesResponseType(typeof(PurchaseOrderSummaryDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateOrder(
        string companyCen,
        [FromBody] CreatePurchaseOrderDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });
        if (string.IsNullOrWhiteSpace(request.SupplierCen))
            return BadRequest(new { message = "supplierCen is required." });
        if (string.IsNullOrWhiteSpace(request.WarehouseCen))
            return BadRequest(new { message = "warehouseCen is required." });

        try
        {
            var result = await _orders.CreateAsync(companyCen, request);
            if (result == null)
                return NotFound(new { message = "Company not found." });

            return CreatedAtAction(
                nameof(GetOrder),
                new { companyCen, orderCen = result.OrderCen },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Obtiene el detalle de una orden de compra</summary>
    [HttpGet("api/purchases/companies/{companyCen}/orders/{orderCen}")]
    [ProducesResponseType(typeof(PurchaseOrderDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrder(string companyCen, string orderCen)
    {
        var result = await _orders.GetAsync(companyCen, orderCen);
        if (result == null)
            return NotFound(new { message = "Purchase order not found." });
        return Ok(result);
    }

    /// <summary>Confirma una orden de compra</summary>
    [HttpPost("api/purchases/companies/{companyCen}/orders/{orderCen}/confirm")]
    [ProducesResponseType(typeof(PurchaseOrderConfirmationDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> ConfirmOrder(string companyCen, string orderCen)
    {
        try
        {
            var result = await _orders.ConfirmAsync(companyCen, orderCen);
            if (result == null)
                return NotFound(new { message = "Purchase order not found." });
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("Only orders in Pending", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("no items", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
