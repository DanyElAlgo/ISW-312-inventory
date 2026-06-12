using Microsoft.AspNetCore.Mvc;
using Purchases.API.DTOs;
using Purchases.API.Exceptions;
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
            throw new NotFoundException("Company not found.");
        return Ok(result);
    }
}
