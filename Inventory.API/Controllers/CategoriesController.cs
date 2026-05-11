using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class CategoriesController : ControllerBase
{
    private readonly InventoryContractService _service;

    public CategoriesController(InventoryContractService service)
    {
        _service = service;
    }

    [HttpGet("companies/{companyCen}/categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(string companyCen)
    {
        var categories = await _service.GetCategoriesAsync(companyCen);
        if (categories == null)
            return NotFound(new { message = "Company not found." });

        return Ok(categories);
    }

    [HttpPost("companies/{companyCen}/categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(string companyCen, [FromBody] CreateCategoryRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var category = await _service.CreateCategoryAsync(companyCen, dto);
            if (category == null)
                return NotFound(new { message = "Company not found." });

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/categories/{categoryCen}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(string companyCen, string categoryCen, [FromBody] UpdateCategoryRequest dto)
    {
        try
        {
            var category = await _service.UpdateCategoryAsync(companyCen, categoryCen, dto);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
