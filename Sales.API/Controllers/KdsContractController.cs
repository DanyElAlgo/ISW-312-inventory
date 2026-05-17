using Microsoft.AspNetCore.Mvc;
using Sales.API.DTOs;
using Sales.API.Services;

namespace Sales.API.Controllers;

[ApiController]
[Tags("KdsContract")]
public class KdsContractController : ControllerBase
{
    private readonly KdsService _kdsService;

    public KdsContractController(KdsService kdsService)
    {
        _kdsService = kdsService;
    }

    /// <summary>Lista equipos KDS</summary>
    [HttpGet("api/sales/companies/{companyCen}/kds/teams")]
    [ProducesResponseType(typeof(List<KdsTeamContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetKdsTeams(string companyCen)
    {
        var result = await _kdsService.GetKdsTeamsAsync();
        return Ok(result);
    }

    /// <summary>Lista items KDS por equipo</summary>
    [HttpGet("api/sales/companies/{companyCen}/kds/teams/{teamCen}/items")]
    [ProducesResponseType(typeof(List<KdsItemContractResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetKdsItemsByTeam(string companyCen, string teamCen)
    {
        var result = await _kdsService.GetKdsItemsByTeamAsync(teamCen);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Actualiza estado de item KDS</summary>
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
            var result = await _kdsService.UpdateItemStatusAsync(ticketItemCen, request.Status);
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
