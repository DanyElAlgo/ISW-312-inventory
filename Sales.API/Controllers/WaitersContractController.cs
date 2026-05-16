using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("WaitersContract")]
public class WaitersContractController : ControllerBase
{
    private readonly PosService _posService;

    public WaitersContractController(PosService posService)
    {
        _posService = posService;
    }

    /// <summary>Lista meseros por empresa</summary>
    /// <remarks>Devuelve las opciones de meseros disponibles para la empresa. Usar para asignar meseros en tickets.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/waiters")]
    [ProducesResponseType(typeof(List<WaiterContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWaiters(string companyCen)
    {
        var waiters = await _posService.GetWaitersAsync();
        if (!waiters.Any())
            return NotFound();
        return Ok(waiters);
    }
}
