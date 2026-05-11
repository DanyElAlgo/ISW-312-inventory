using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;

namespace Inventory.API.Services;

/// <summary>
/// Facade service that delegates to domain-specific inventory services.
/// This service maintains backward compatibility while delegating operations to specialized services.
/// 
/// REFACTORED STRUCTURE (May 2026):
/// - CompaniesService: Companies and dashboard operations
/// - CategoriesService: Category management
/// - UnitsService: Unit management
/// - WarehousesService: Warehouse management
/// - ProductsService: Product management and status
/// - StockService: Stock-level operations
/// - DocumentsService: Inventory documents
/// - KardexService: Movement history
/// - StockValidationService: Stock validation
/// - StockConsumeService: Stock consumption for sales
/// </summary>
public class InventoryContractService
{
    private readonly CompaniesService _companiesService;
    private readonly CategoriesService _categoriesService;
    private readonly UnitsService _unitsService;
    private readonly WarehousesService _warehousesService;
    private readonly ProductsService _productsService;
    private readonly StockService _stockService;
    private readonly DocumentsService _documentsService;
    private readonly KardexService _kardexService;
    private readonly StockValidationService _stockValidationService;
    private readonly StockConsumeService _stockConsumeService;

    public InventoryContractService(
        InventoryDbContext context,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitRepository unitRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseProductRepository warehouseProductRepository)
    {
        _companiesService = new CompaniesService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _categoriesService = new CategoriesService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _unitsService = new UnitsService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _warehousesService = new WarehousesService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _productsService = new ProductsService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _stockService = new StockService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _documentsService = new DocumentsService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _kardexService = new KardexService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _stockValidationService = new StockValidationService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
        _stockConsumeService = new StockConsumeService(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository);
    }

    // Companies delegates
    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync() => await _companiesService.GetCompaniesAsync();

    public async Task<InventoryDashboardDto?> GetDashboardAsync(string companyCen) => await _companiesService.GetDashboardAsync(companyCen);

    // Categories delegates
    public async Task<IReadOnlyList<CategoryDto>?> GetCategoriesAsync(string companyCen) => await _categoriesService.GetCategoriesAsync(companyCen);

    public async Task<CategoryDto?> CreateCategoryAsync(string companyCen, CreateCategoryRequest dto) => await _categoriesService.CreateCategoryAsync(companyCen, dto);

    public async Task<CategoryDto?> UpdateCategoryAsync(string companyCen, string categoryCen, UpdateCategoryRequest dto) => await _categoriesService.UpdateCategoryAsync(companyCen, categoryCen, dto);

    // Units delegates
    public async Task<IReadOnlyList<UnitDto>?> GetUnitsAsync(string companyCen) => await _unitsService.GetUnitsAsync(companyCen);

    public async Task<UnitDto?> CreateUnitAsync(string companyCen, CreateUnitRequest dto) => await _unitsService.CreateUnitAsync(companyCen, dto);

    public async Task<UnitDto?> UpdateUnitAsync(string companyCen, string unitCen, UpdateUnitRequest dto) => await _unitsService.UpdateUnitAsync(companyCen, unitCen, dto);

    // Warehouses delegates
    public async Task<IReadOnlyList<WarehouseDto>?> GetWarehousesAsync(string companyCen) => await _warehousesService.GetWarehousesAsync(companyCen);

    public async Task<WarehouseDto?> CreateWarehouseAsync(string companyCen, CreateWarehouseRequest dto) => await _warehousesService.CreateWarehouseAsync(companyCen, dto);

    public async Task<WarehouseDto?> UpdateWarehouseAsync(string companyCen, string warehouseCen, UpdateWarehouseRequest dto) => await _warehousesService.UpdateWarehouseAsync(companyCen, warehouseCen, dto);

    // Products delegates
    public async Task<IReadOnlyList<ProductDto>?> GetProductsAsync(string companyCen, string? search, string? categoryCen, string? status) => await _productsService.GetProductsAsync(companyCen, search, categoryCen, status);

    public async Task<ProductDto?> GetProductAsync(string companyCen, string productCen) => await _productsService.GetProductAsync(companyCen, productCen);

    public async Task<ProductDto?> CreateProductAsync(string companyCen, CreateProductRequest dto) => await _productsService.CreateProductAsync(companyCen, dto);

    public async Task<ProductDto?> UpdateProductAsync(string companyCen, string productCen, UpdateProductRequest dto) => await _productsService.UpdateProductAsync(companyCen, productCen, dto);

    public async Task<ProductDto?> UpdateProductStatusAsync(string companyCen, string productCen, UpdateProductStatusRequest dto) => await _productsService.UpdateProductStatusAsync(companyCen, productCen, dto);

    // Stock delegates
    public async Task<IReadOnlyList<StockItemDto>?> GetStockAsync(string companyCen, string? productCen, string? warehouseCen) => await _stockService.GetStockAsync(companyCen, productCen, warehouseCen);

    public async Task<StockAdjustmentResponse?> CreateAdjustmentAsync(string companyCen, StockAdjustmentRequest dto) => await _stockService.CreateAdjustmentAsync(companyCen, dto);

    // Kardex delegates
    public async Task<IReadOnlyList<KardexMovementDto>?> GetKardexAsync(string companyCen, string productCen, string? warehouseCen, DateTime? from, DateTime? to) => await _kardexService.GetKardexAsync(companyCen, productCen, warehouseCen, from, to);

    // Documents delegates
    public async Task<InventoryDocumentDto?> CreateDocumentAsync(string companyCen, InventoryDocumentCreateRequest dto) => await _documentsService.CreateDocumentAsync(companyCen, dto);

    public async Task<IReadOnlyList<InventoryDocumentDto>?> GetDocumentsAsync(string companyCen, string? documentType, DateTime? from, DateTime? to) => await _documentsService.GetDocumentsAsync(companyCen, documentType, from, to);

    // Stock validation delegates
    public async Task<StockValidationResponse?> ValidateStockAsync(string companyCen, StockValidationRequest dto) => await _stockValidationService.ValidateStockAsync(companyCen, dto);

    // Stock consumption delegates
    public async Task<StockConsumeResponse?> ConsumeStockAsync(string companyCen, StockConsumeRequest dto) => await _stockConsumeService.ConsumeStockAsync(companyCen, dto);
}
