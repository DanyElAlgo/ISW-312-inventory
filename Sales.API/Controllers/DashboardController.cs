using Sales.API.DTOs;
using Sales.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesDashboardDto>> GetSalesDashboard()
    {
        var result = await _service.GetSalesDashboardAsync();
        return Ok(result);
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts([FromQuery] int limit = 10)
    {
        var result = await _service.GetTopProductsAsync(limit);
        return Ok(result);
    }

    [HttpGet("stock-alerts")]
    public async Task<ActionResult<StockAlertsDashboardDto>> GetStockAlerts()
    {
        var result = await _service.GetStockAlertsDashboardAsync();
        return Ok(result);
    }

    [HttpGet("kds-status")]
    public async Task<ActionResult<KdsStatusSummaryDto>> GetKdsStatus()
    {
        var result = await _service.GetKdsStatusSummaryAsync();
        return Ok(result);
    }
}
