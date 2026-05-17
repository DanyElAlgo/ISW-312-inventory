using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("WaitersContract")]
public class WaitersContractController : ControllerBase
{
    private readonly WaitersService _waitersService;

    public WaitersContractController(WaitersService waitersService)
    {
        _waitersService = waitersService;
    }

    /// <summary>Lista meseros por empresa</summary>
    [HttpGet("api/sales/companies/{companyCen}/waiters")]
    [ProducesResponseType(typeof(List<WaiterContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWaiters(string companyCen)
    {
        var waiters = await _waitersService.GetWaitersAsync();
        if (!waiters.Any())
            return NotFound();
        return Ok(waiters);
    }
}
