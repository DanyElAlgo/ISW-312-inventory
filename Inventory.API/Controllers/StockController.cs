using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class StockController : ControllerBase
{
    private readonly StockService _stockService;
    private readonly StockValidationService _validationService;
    private readonly StockConsumeService _consumeService;

    public StockController(
        StockService stockService,
        StockValidationService validationService,
        StockConsumeService consumeService)
    {
        _stockService = stockService;
        _validationService = validationService;
        _consumeService = consumeService;
    }

    [HttpGet("companies/{companyCen}/stock")]
    public async Task<ActionResult<IReadOnlyList<StockItemDto>>> GetStock(
        string companyCen,
        [FromQuery] string? productCen,
        [FromQuery] string? warehouseCen)
    {
        var stock = await _stockService.GetStockAsync(companyCen, productCen, warehouseCen);
        if (stock == null)
            return NotFound(new { message = "Company, product, or warehouse not found." });

        return Ok(stock);
    }

    [HttpPost("companies/{companyCen}/stock/increase")]
    public async Task<ActionResult<StockIncreaseResponse>> IncreaseStock(
        string companyCen,
        [FromBody] StockIncreaseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _stockService.IncreaseStockAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("companies/{companyCen}/stock/adjustments")]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateAdjustment(
        string companyCen,
        [FromBody] StockAdjustmentRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _stockService.CreateAdjustmentAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("companies/{companyCen}/stock/validate")]
    public async Task<ActionResult<StockValidationResponse>> ValidateStock(
        string companyCen,
        [FromBody] StockValidationRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _validationService.ValidateStockAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("companies/{companyCen}/stock/consume")]
    public async Task<ActionResult<StockConsumeResponse>> ConsumeStock(
        string companyCen,
        [FromBody] StockConsumeRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _consumeService.ConsumeStockAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            if (!result.Success)
                return Conflict(result);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
