using Microsoft.AspNetCore.Mvc;
using Inventory.API.DTOs;
using Inventory.API.Services;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryGetDto>>> GetAll()
    {
        var categories = await _service.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryGetDto>> GetById(int id)
    {
        var category = await _service.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryGetDto>> Create([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var category = await _service.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryGetDto>> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var category = await _service.UpdateCategoryAsync(id, dto);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteCategoryAsync(id);
        if (!success)
            return NotFound(new { message = "Category not found" });

        return NoContent();
    }
}
