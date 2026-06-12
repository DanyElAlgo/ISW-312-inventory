using Microsoft.AspNetCore.Mvc;
using Purchases.Domain.Enums;
using Purchases.API.DTOs;
using Purchases.API.Exceptions;
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
        [FromQuery] PurchaseStatusEnum? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortDescending = true)
    {
        var result = await _orders.ListAsync(companyCen, status, page, pageSize, sortDescending);
        if (result == null)
            throw new NotFoundException("Company not found.");
        return Ok(result);
    }

    /// <summary>Crea una orden de compra</summary>
    [HttpPost("api/purchases/companies/{companyCen}/orders")]
    [ProducesResponseType(typeof(string), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PurchaseOrderSummaryDto>> CreateOrder(
        string companyCen,
        [FromBody] CreatePurchaseOrderDto request)
    {
        if (request == null)
            throw new InvalidOperationException("Request body is required.");
        if (string.IsNullOrWhiteSpace(request.SupplierCen))
            throw new InvalidOperationException("supplierCen is required.");
        if (string.IsNullOrWhiteSpace(request.WarehouseCen))
            throw new InvalidOperationException("warehouseCen is required.");

        var result = await _orders.CreateAsync(companyCen, request);
        if (result == null)
            throw new NotFoundException("Company not found.");

        return CreatedAtAction(
            nameof(GetOrder),
            new { companyCen, orderCen = result.OrderCen },
            result);
    }

    /// <summary>Obtiene el detalle de una orden de compra</summary>
    [HttpGet("api/purchases/companies/{companyCen}/orders/{orderCen}")]
    [ProducesResponseType(typeof(PurchaseOrderDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrder(string companyCen, string orderCen)
    {
        var result = await _orders.GetAsync(companyCen, orderCen);
        if (result == null)
            throw new NotFoundException("Purchase order not found.");
        return Ok(result);
    }

    /// <summary>Confirma una orden de compra</summary>
    [HttpPost("api/purchases/companies/{companyCen}/orders/{orderCen}/confirm")]
    [ProducesResponseType(typeof(PurchaseOrderConfirmationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> ConfirmOrder(string companyCen, string orderCen)
    {
        var result = await _orders.ConfirmAsync(companyCen, orderCen);
        if (result == null)
            throw new NotFoundException("Purchase order not found.");
        return Ok(result);
    }
}
