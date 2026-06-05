using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("DefaultWarehouseContract")]
public class DefaultWarehouseContractController : ControllerBase
{
    private readonly DefaultWarehouseService _service;

    public DefaultWarehouseContractController(DefaultWarehouseService service)
    {
        _service = service;
    }

    /// <summary>Obtiene la bodega por defecto de la empresa</summary>
    [HttpGet("api/sales/companies/{companyCen}/default-warehouse")]
    [ProducesResponseType(typeof(DefaultWarehouseContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDefault(string companyCen)
    {
        var result = await _service.GetDefaultAsync(companyCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Define la bodega por defecto de la empresa</summary>
    [HttpPut("api/sales/companies/{companyCen}/default-warehouse")]
    [ProducesResponseType(typeof(DefaultWarehouseContractResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetDefault(
        string companyCen,
        [FromBody] SetDefaultWarehouseContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WarehouseCen))
            return BadRequest("warehouseCen is required.");

        try
        {
            var result = await _service.SetDefaultAsync(companyCen, request.WarehouseCen);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
