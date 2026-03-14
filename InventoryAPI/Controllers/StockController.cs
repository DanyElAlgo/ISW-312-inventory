using Microsoft.AspNetCore.Mvc;
using InventoryAPI.DTOs;
using InventoryAPI.Services;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly ProductService _service;

    public StockController(ProductService service)
    {
        _service = service;
    }

    [HttpGet("warehouse/{warehouseId}/history")]
    public async Task<ActionResult<IEnumerable<KardexGetDto>>> GetWarehouseHistory(int warehouseId)
    {
        var history = await _service.GetWarehouseHistoryAsync(warehouseId);
        return Ok(history);
    }

    [HttpGet("product/{productId}/warehouse/{warehouseId}/history")]
    public async Task<ActionResult<IEnumerable<KardexGetDto>>> GetProductWarehouseHistory(int productId, int warehouseId)
    {
        var history = await _service.GetProductWarehouseHistoryAsync(productId, warehouseId);
        return Ok(history);
    }

    [HttpGet("product/{productId}/history")]
    public async Task<ActionResult<IEnumerable<KardexGetDto>>> GetProductHistory(int productId)
    {
        var history = await _service.GetProductHistoryAsync(productId);
        return Ok(history);
    }
}
