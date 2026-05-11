using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class CompaniesController : ControllerBase
{
    private readonly InventoryContractService _service;

    public CompaniesController(InventoryContractService service)
    {
        _service = service;
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies()
    {
        var companies = await _service.GetCompaniesAsync();
        return Ok(companies);
    }

    [HttpGet("companies/{companyCen}/dashboard")]
    public async Task<ActionResult<InventoryDashboardDto>> GetDashboard(string companyCen)
    {
        var dashboard = await _service.GetDashboardAsync(companyCen);
        if (dashboard == null)
            return NotFound(new { message = "Company not found." });

        return Ok(dashboard);
    }
}
