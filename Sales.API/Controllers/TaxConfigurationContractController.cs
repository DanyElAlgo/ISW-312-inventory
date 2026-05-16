using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sales.API.DTOs;
using Sales.API.HttpClients;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("TaxConfigurationContract")]
public class TaxConfigurationContractController : ControllerBase
{
    private readonly PosService _posService;
    private readonly InventoryIntegrationOptions _options;

    public TaxConfigurationContractController(
        PosService posService,
        IOptions<InventoryIntegrationOptions> options)
    {
        _posService = posService;
        _options = options.Value;
    }

    /// <summary>Obtiene configuracion de impuestos</summary>
    /// <remarks>Devuelve el porcentaje global de impuesto configurado para la empresa. Usar para calcular totales en ventas.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/tax-configuration")]
    [ProducesResponseType(typeof(TaxConfigurationContractResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTaxConfiguration(string companyCen)
    {
        var config = await _posService.GetGlobalTaxAsync();
        return Ok(new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config.TaxRate
        });
    }

    /// <summary>Actualiza configuracion de impuestos</summary>
    /// <remarks>Registra o actualiza el porcentaje global de impuesto. Usar cuando se cambien reglas fiscales de la empresa.</remarks>
    [HttpPut("api/sales/companies/{companyCen}/tax-configuration")]
    [ProducesResponseType(typeof(TaxConfigurationContractResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTaxConfiguration(
        string companyCen,
        [FromBody] UpdateTaxConfigurationContractRequest request)
    {
        if (request.GlobalTaxPercentage < 0)
            return BadRequest("globalTaxPercentage cannot be negative.");

        try
        {
            var updated = await _posService.UpdateGlobalTaxAsync(
                new GlobalTaxConfigDto { TaxRate = request.GlobalTaxPercentage });

            return Ok(new TaxConfigurationContractResponse
            {
                CompanyCen = companyCen,
                GlobalTaxPercentage = updated.TaxRate
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
