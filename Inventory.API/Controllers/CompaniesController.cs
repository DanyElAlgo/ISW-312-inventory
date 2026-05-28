using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class CompaniesController : ControllerBase
{
    private readonly CompaniesService _service;

    public CompaniesController(CompaniesService service)
    {
        _service = service;
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies()
    {
        var companies = await _service.GetCompaniesAsync();
        return Ok(companies);
    }

    [HttpGet("companies/{companyCen}")]
    public async Task<ActionResult<CompanyLookupContractDto>> GetCompany(string companyCen)
    {
        var company = await _service.GetCompanyAsync(companyCen);
        if (company == null)
            return NotFound(new { message = "Company not found." });

        return Ok(company);
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
