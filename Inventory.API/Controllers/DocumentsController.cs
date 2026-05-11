using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class DocumentsController : ControllerBase
{
    private readonly InventoryContractService _service;

    public DocumentsController(InventoryContractService service)
    {
        _service = service;
    }

    [HttpPost("companies/{companyCen}/documents")]
    public async Task<ActionResult<InventoryDocumentDto>> CreateDocument(
        string companyCen,
        [FromBody] InventoryDocumentCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateDocumentAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message == "INSUFFICIENT_STOCK")
        {
            var validation = await _service.ValidateStockAsync(companyCen, new StockValidationRequest
            {
                WarehouseCen = dto.WarehouseCen,
                Source = "DOCUMENT",
                ReferenceCen = dto.ExternalReference,
                Items = dto.Lines.Select(l => new StockValidationItemDto
                {
                    ProductCen = l.ProductCen,
                    Quantity = l.Quantity
                }).ToList()
            });

            return Conflict(new
            {
                message = "Insufficient stock to register document.",
                requirements = validation?.Requirements ?? new List<StockRequirementDto>()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies/{companyCen}/documents")]
    public async Task<ActionResult<IReadOnlyList<InventoryDocumentDto>>> GetDocuments(
        string companyCen,
        [FromQuery] string? documentType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var documents = await _service.GetDocumentsAsync(companyCen, documentType, from, to);
        if (documents == null)
            return NotFound(new { message = "Company not found." });

        return Ok(documents);
    }
}
