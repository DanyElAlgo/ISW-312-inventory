namespace Inventory.API.Controllers;

/// <summary>
/// DEPRECATED: This controller has been refactored into domain-specific controllers.
/// 
/// The endpoints from InventoryContractController have been split into:
/// - CompaniesController: Company and dashboard endpoints
/// - CategoriesController: Category management endpoints
/// - UnitsController: Unit management endpoints
/// - WarehousesController: Warehouse management endpoints
/// - ProductsController: Product management endpoints
/// - StockController: Stock operations (adjustments, validation, consumption)
/// - KardexController: Inventory movement tracking
/// - DocumentsController: Document creation and retrieval
/// 
/// All original API routes remain unchanged. This file is kept for reference only.
/// </summary>
[Obsolete("Use domain-specific controllers (CompaniesController, CategoriesController, etc.) instead.", true)]
public class InventoryContractController
{
}
