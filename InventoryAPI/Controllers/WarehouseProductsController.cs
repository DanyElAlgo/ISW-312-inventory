using Microsoft.AspNetCore.Mvc;
using InventoryAPI.DTOs;
using InventoryAPI.Services;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseProductsController : ControllerBase
{
    private readonly WarehouseProductService _service;

    public WarehouseProductsController(WarehouseProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseProductGetDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseProductGetDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound(new { message = "Warehouse product not found" });

        return Ok(item);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<WarehouseProductGetDto>>> GetByWarehouse(int warehouseId)
    {
        var items = await _service.GetByWarehouseIdAsync(warehouseId);
        return Ok(items);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<WarehouseProductGetDto>>> GetByProduct(int productId)
    {
        var items = await _service.GetByProductIdAsync(productId);
        return Ok(items);
    }

    [HttpGet("stock/low")]
    public async Task<ActionResult<IEnumerable<WarehouseProductGetDto>>> GetLowStockItems()
    {
        var items = await _service.GetLowStockItemsAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseProductGetDto>> Create([FromBody] WarehouseProductCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WarehouseProductGetDto>> Update(int id, [FromBody] WarehouseProductUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = await _service.UpdateAsync(id, dto);
        if (item == null)
            return NotFound(new { message = "Warehouse product not found" });

        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = "Warehouse product not found" });

        return NoContent();
    }
}
