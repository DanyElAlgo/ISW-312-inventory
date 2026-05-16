using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("KdsContract")]
public class KdsContractController : ControllerBase
{
    private readonly PosService _posService;

    public KdsContractController(PosService posService)
    {
        _posService = posService;
    }

    /// <summary>Lista equipos KDS</summary>
    /// <remarks>Devuelve los equipos KDS configurados para la empresa. Usar para seleccionar el equipo en vistas de cocina.</remarks>
    [HttpGet("api/sales/companies/{companyCen}/kds/teams")]
    [ProducesResponseType(typeof(List<KdsTeamContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetKdsTeams(string companyCen)
    {
        var result = await _posService.GetKdsTeamsAsync();
        return Ok(result);
    }

    /// <summary>Lista items KDS por equipo</summary>
    /// <remarks>
    /// Devuelve los items activos del equipo KDS en las ultimas 48 horas.
    /// Usar para tableros de preparacion y monitoreo de pedidos.
    /// Integra con el API de Inventario para enriquecer datos de producto.
    /// </remarks>
    [HttpGet("api/sales/companies/{companyCen}/kds/teams/{teamCen}/items")]
    [ProducesResponseType(typeof(List<KdsItemContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetKdsItemsByTeam(string companyCen, string teamCen)
    {
        var result = await _posService.GetKdsItemsByTeamAsync(teamCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Actualiza estado de item KDS</summary>
    /// <remarks>Cambia el estado operativo de un item (created/preparing/delivered/canceled). Usar para reflejar el avance en cocina.</remarks>
    [HttpPatch("api/sales/companies/{companyCen}/kds/items/{ticketItemCen}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateKdsItemStatus(
        string companyCen,
        string ticketItemCen,
        [FromBody] UpdateKdsItemStatusContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest("status is required.");

        try
        {
            var result = await _posService.UpdateKdsItemStatusContractAsync(ticketItemCen, request.Status);
            if (result == null)
                return NotFound();
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
