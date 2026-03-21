using Microsoft.AspNetCore.Mvc;
using InventoryAPI.DTOs;
using InventoryAPI.Services;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly ProductService _service;
    private readonly WarehouseProductService _warehouseProductService;

    public StockController(ProductService service, WarehouseProductService warehouseProductService)
    {
        _service = service;
        _warehouseProductService = warehouseProductService;
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

    [HttpPost("initial")]
    public async Task<ActionResult<StockOperationResultDto>> SetInitialStock([FromBody] StockSetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _warehouseProductService.SetInitialStockAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("adjust")]
    public async Task<ActionResult<StockOperationResultDto>> AdjustStock([FromBody] StockChangeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _warehouseProductService.AdjustStockAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("warehouse/{warehouseId}/product/{productId}/out-of-stock")]
    public async Task<ActionResult<StockOperationResultDto>> SetOutOfStockStatus(int warehouseId, int productId, [FromBody] OutOfStockUpdateDto dto)
    {
        var result = await _warehouseProductService.SetOutOfStockAsync(warehouseId, productId, dto.IsOutOfStock);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
