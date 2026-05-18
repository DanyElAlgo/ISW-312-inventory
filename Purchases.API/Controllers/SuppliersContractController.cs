using Microsoft.AspNetCore.Mvc;
using Purchases.API.DTOs;
using Purchases.API.Services;

namespace Purchases.API.Controllers;

[ApiController]
[Tags("Supplier")]
public class SuppliersContractController : ControllerBase
{
    private readonly SuppliersService _suppliers;

    public SuppliersContractController(SuppliersService suppliers)
    {
        _suppliers = suppliers;
    }

    /// <summary>Lista proveedores activos de una empresa</summary>
    [HttpGet("api/purchases/companies/{companyCen}/suppliers")]
    [ProducesResponseType(typeof(List<SupplierDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListSuppliers(string companyCen)
    {
        var result = await _suppliers.ListAsync(companyCen);
        if (result == null)
            return NotFound(new { message = "Company not found." });
        return Ok(result);
    }

    /// <summary>Crea un proveedor</summary>
    [HttpPost("api/purchases/companies/{companyCen}/suppliers")]
    [ProducesResponseType(typeof(SupplierDetailDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateSupplier(
        string companyCen,
        [FromBody] CreateSupplierDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "name is required." });

        try
        {
            var result = await _suppliers.CreateAsync(companyCen, request);
            if (result == null)
                return NotFound(new { message = "Company not found." });

            return CreatedAtAction(
                nameof(ListSuppliers),
                new { companyCen },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Actualiza un proveedor</summary>
    [HttpPut("api/purchases/companies/{companyCen}/suppliers/{supplierCen}")]
    [ProducesResponseType(typeof(SupplierDetailDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSupplier(
        string companyCen,
        string supplierCen,
        [FromBody] UpdateSupplierDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "name is required." });

        try
        {
            var result = await _suppliers.UpdateAsync(companyCen, supplierCen, request);
            if (result == null)
                return NotFound(new { message = "Supplier or company not found." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Desactiva un proveedor (soft delete)</summary>
    [HttpDelete("api/purchases/companies/{companyCen}/suppliers/{supplierCen}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteSupplier(string companyCen, string supplierCen)
    {
        var result = await _suppliers.DeleteAsync(companyCen, supplierCen);
        if (result == null)
            return NotFound(new { message = "Supplier or company not found." });
        return NoContent();
    }
}
