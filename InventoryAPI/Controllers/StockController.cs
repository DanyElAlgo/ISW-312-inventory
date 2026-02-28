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

    [HttpPost("set")]
    public async Task<ActionResult<StockOperationResultDto>> SetStock([FromBody] StockSetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.SetStockAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<ActionResult<StockOperationResultDto>> AddStock([FromBody] StockChangeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.AddStockAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("subtract")]
    public async Task<ActionResult<StockOperationResultDto>> SubtractStock([FromBody] StockChangeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.SubtractStockAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<StockOperationResultDto>> TransferStock([FromBody] StockTransferDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.TransferStockAsync(dto.ProductId, dto.SourceWarehouseId, dto.DestinationWarehouseId, dto.Quantity, dto.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
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
}
