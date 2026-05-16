using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("DashboardContract")]
public class DashboardContractController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardContractController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Obtiene ventas diarias</summary>
    /// <remarks>Devuelve el resumen de ventas del dia actual para la empresa. Usar en dashboards ejecutivos de ventas.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/dashboard/daily-sales")]
    [ProducesResponseType(typeof(DailySalesDashboardDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDailySales(string companyCen)
    {
        var result = await _dashboardService.GetDailySalesDashboardAsync();
        return Ok(result);
    }

    /// <summary>Obtiene top productos vendidos</summary>
    /// <remarks>
    /// Devuelve los productos mas vendidos del dia actual.
    /// Usar para analitica rapida y reportes de desempeño.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpGet("api/sales/companies/{companyCen}/dashboard/top-products")]
    [ProducesResponseType(typeof(List<TopProductDashboardContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTopProducts(string companyCen, [FromQuery] int topN = 10)
    {
        var result = await _dashboardService.GetTopProductsDashboardAsync(topN);
        return Ok(result);
    }

    /// <summary>Obtiene estado del KDS</summary>
    /// <remarks>Devuelve el estado operativo del sistema KDS para la empresa. Usar para indicadores de cocina o tableros de servicio.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/dashboard/kds-status")]
    [ProducesResponseType(typeof(KdsStatusDashboardDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetKdsStatus(string companyCen)
    {
        var result = await _dashboardService.GetKdsStatusDashboardAsync();
        return Ok(result);
    }
}
