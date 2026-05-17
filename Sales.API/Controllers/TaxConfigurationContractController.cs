using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("TaxConfigurationContract")]
public class TaxConfigurationContractController : ControllerBase
{
    private readonly TaxConfigurationService _taxService;

    public TaxConfigurationContractController(TaxConfigurationService taxService)
    {
        _taxService = taxService;
    }

    /// <summary>Obtiene configuracion de impuestos</summary>
    [HttpGet("api/sales/companies/{companyCen}/tax-configuration")]
    [ProducesResponseType(typeof(TaxConfigurationContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaxConfiguration(string companyCen)
    {
        var config = await _taxService.GetAsync(companyCen);
        return Ok(config);
    }

    /// <summary>Actualiza configuracion de impuestos</summary>
    [HttpPut("api/sales/companies/{companyCen}/tax-configuration")]
    [ProducesResponseType(typeof(TaxConfigurationContractResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTaxConfiguration(
        string companyCen,
        [FromBody] UpdateTaxConfigurationContractRequest request)
    {
        try
        {
            var updated = await _taxService.UpdateAsync(companyCen, request);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
