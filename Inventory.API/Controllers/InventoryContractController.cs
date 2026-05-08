using Inventory.API.DTOs.Contract;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryContractController : ControllerBase
{
    private readonly InventoryContractService _service;

    public InventoryContractController(InventoryContractService service)
    {
        _service = service;
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies()
    {
        var companies = await _service.GetCompaniesAsync();
        return Ok(companies);
    }

    [HttpGet("companies/{companyCen}/dashboard")]
    public async Task<ActionResult<InventoryDashboardDto>> GetDashboard(string companyCen)
    {
        var dashboard = await _service.GetDashboardAsync(companyCen);
        if (dashboard == null)
            return NotFound(new { message = "Company not found." });

        return Ok(dashboard);
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

    [HttpGet("companies/{companyCen}/units")]
    public async Task<ActionResult<IReadOnlyList<UnitDto>>> GetUnits(string companyCen)
    {
        var units = await _service.GetUnitsAsync(companyCen);
        if (units == null)
            return NotFound(new { message = "Company not found." });

        return Ok(units);
    }

    [HttpPost("companies/{companyCen}/units")]
    public async Task<ActionResult<UnitDto>> CreateUnit(string companyCen, [FromBody] CreateUnitRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var unit = await _service.CreateUnitAsync(companyCen, dto);
            if (unit == null)
                return NotFound(new { message = "Company not found." });

            return Ok(unit);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/units/{unitCen}")]
    public async Task<ActionResult<UnitDto>> UpdateUnit(string companyCen, string unitCen, [FromBody] UpdateUnitRequest dto)
    {
        try
        {
            var unit = await _service.UpdateUnitAsync(companyCen, unitCen, dto);
            if (unit == null)
                return NotFound(new { message = "Unit not found." });

            return Ok(unit);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies/{companyCen}/warehouses")]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetWarehouses(string companyCen)
    {
        var warehouses = await _service.GetWarehousesAsync(companyCen);
        if (warehouses == null)
            return NotFound(new { message = "Company not found." });

        return Ok(warehouses);
    }

    [HttpPost("companies/{companyCen}/warehouses")]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse(string companyCen, [FromBody] CreateWarehouseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var warehouse = await _service.CreateWarehouseAsync(companyCen, dto);
            if (warehouse == null)
                return NotFound(new { message = "Company not found." });

            return Ok(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/warehouses/{warehouseCen}")]
    public async Task<ActionResult<WarehouseDto>> UpdateWarehouse(string companyCen, string warehouseCen, [FromBody] UpdateWarehouseRequest dto)
    {
        try
        {
            var warehouse = await _service.UpdateWarehouseAsync(companyCen, warehouseCen, dto);
            if (warehouse == null)
                return NotFound(new { message = "Warehouse not found." });

            return Ok(warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies/{companyCen}/products")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? status)
    {
        var products = await _service.GetProductsAsync(companyCen, search, categoryCen, status);
        if (products == null)
            return NotFound(new { message = "Company or category not found." });

        return Ok(products);
    }

    [HttpGet("companies/{companyCen}/products/{productCen}")]
    public async Task<ActionResult<ProductDto>> GetProduct(string companyCen, string productCen)
    {
        var product = await _service.GetProductAsync(companyCen, productCen);
        if (product == null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    [HttpPost("companies/{companyCen}/products")]
    public async Task<ActionResult<ProductDto>> CreateProduct(string companyCen, [FromBody] CreateProductRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await _service.CreateProductAsync(companyCen, dto);
            if (product == null)
                return NotFound(new { message = "Company not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("companies/{companyCen}/products/{productCen}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(string companyCen, string productCen, [FromBody] UpdateProductRequest dto)
    {
        try
        {
            var product = await _service.UpdateProductAsync(companyCen, productCen, dto);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("companies/{companyCen}/products/{productCen}/status")]
    public async Task<ActionResult<ProductDto>> UpdateProductStatus(
        string companyCen,
        string productCen,
        [FromBody] UpdateProductStatusRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var product = await _service.UpdateProductStatusAsync(companyCen, productCen, dto);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies/{companyCen}/stock")]
    public async Task<ActionResult<IReadOnlyList<StockItemDto>>> GetStock(
        string companyCen,
        [FromQuery] string? productCen,
        [FromQuery] string? warehouseCen)
    {
        var stock = await _service.GetStockAsync(companyCen, productCen, warehouseCen);
        if (stock == null)
            return NotFound(new { message = "Company, product, or warehouse not found." });

        return Ok(stock);
    }

    [HttpPost("companies/{companyCen}/stock/adjustments")]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateAdjustment(
        string companyCen,
        [FromBody] StockAdjustmentRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.CreateAdjustmentAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("companies/{companyCen}/products/{productCen}/kardex")]
    public async Task<ActionResult<IReadOnlyList<KardexMovementDto>>> GetKardex(
        string companyCen,
        string productCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var kardex = await _service.GetKardexAsync(companyCen, productCen, warehouseCen, from, to);
        if (kardex == null)
            return NotFound(new { message = "Company, product, or warehouse not found." });

        return Ok(kardex);
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

    [HttpPost("companies/{companyCen}/stock/validate")]
    public async Task<ActionResult<StockValidationResponse>> ValidateStock(
        string companyCen,
        [FromBody] StockValidationRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.ValidateStockAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("companies/{companyCen}/stock/consume")]
    public async Task<ActionResult<StockConsumeResponse>> ConsumeStock(
        string companyCen,
        [FromBody] StockConsumeRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.ConsumeStockAsync(companyCen, dto);
            if (result == null)
                return NotFound(new { message = "Company or warehouse not found." });

            if (!result.Success)
                return Conflict(result);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
