using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Services;

public class InventoryContractService
{
    private const int ActiveStatusId = 1;
    private const int OutOfStockStatusId = 4;
    private readonly InventoryDbContext _context;

    public InventoryContractService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync()
    {
        var businesses = await _context.Businesses
            .OrderBy(b => b.Name)
            .ToListAsync();

        var updated = false;
        foreach (var business in businesses)
        {
            if (string.IsNullOrWhiteSpace(business.Cen))
            {
                business.Cen = BuildCen("COMP", business.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return businesses.Select(b => new CompanyDto
        {
            CompanyCen = b.Cen ?? BuildCen("COMP", b.Id),
            Name = b.Name ?? string.Empty,
            IsActive = b.IsActive
        }).ToList();
    }

    public async Task<InventoryDashboardDto?> GetDashboardAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var productStocks = await _context.Products
            .Where(p => p.BusinessId == business.Id)
            .Select(p => new
            {
                p.Id,
                p.ReorderLevel,
                TotalStock = p.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0)
            })
            .ToListAsync();

        var totalProducts = productStocks.Count;
        var totalStockQuantity = productStocks.Sum(p => p.TotalStock);
        var lowStockCount = productStocks.Count(p => p.ReorderLevel > 0 && p.TotalStock > 0 && p.TotalStock <= p.ReorderLevel);
        var outOfStockCount = productStocks.Count(p => p.TotalStock <= 0);

        return new InventoryDashboardDto
        {
            CompanyCen = business.Cen ?? BuildCen("COMP", business.Id),
            TotalProducts = totalProducts,
            TotalStockQuantity = totalStockQuantity,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount
        };
    }

    public async Task<IReadOnlyList<CategoryDto>?> GetCategoriesAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var categories = await _context.Categories
            .Where(c => c.BusinessId == business.Id)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var updated = false;
        foreach (var category in categories)
        {
            if (string.IsNullOrWhiteSpace(category.Cen))
            {
                category.Cen = BuildCen("CAT", category.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return categories.Select(MapCategory).ToList();
    }

    public async Task<CategoryDto?> CreateCategoryAsync(string companyCen, CreateCategoryRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Category name is required.");

        var category = new Category
        {
            BusinessId = business.Id,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        category.Cen = BuildCen("CAT", category.Id);
        await _context.SaveChangesAsync();

        return MapCategory(category);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(string companyCen, string categoryCen, UpdateCategoryRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var category = await ResolveCategoryAsync(business.Id, categoryCen);
        if (category == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            category.Name = dto.Name.Trim();

        if (dto.Description != null)
            category.Description = dto.Description;

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return MapCategory(category);
    }

    public async Task<IReadOnlyList<UnitDto>?> GetUnitsAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var units = await _context.Units
            .Where(u => u.BusinessId == business.Id)
            .OrderBy(u => u.Name)
            .ToListAsync();

        var updated = false;
        foreach (var unit in units)
        {
            if (string.IsNullOrWhiteSpace(unit.Cen))
            {
                unit.Cen = BuildCen("UNIT", unit.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return units.Select(MapUnit).ToList();
    }

    public async Task<UnitDto?> CreateUnitAsync(string companyCen, CreateUnitRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Unit name is required.");

        var unit = new Unit
        {
            BusinessId = business.Id,
            Name = dto.Name.Trim(),
            Abbreviation = dto.Abbreviation,
            IsActive = true
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        unit.Cen = BuildCen("UNIT", unit.Id);
        await _context.SaveChangesAsync();

        return MapUnit(unit);
    }

    public async Task<UnitDto?> UpdateUnitAsync(string companyCen, string unitCen, UpdateUnitRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var unit = await ResolveUnitAsync(business.Id, unitCen);
        if (unit == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            unit.Name = dto.Name.Trim();

        if (dto.Abbreviation != null)
            unit.Abbreviation = dto.Abbreviation;

        if (dto.IsActive.HasValue)
            unit.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return MapUnit(unit);
    }

    public async Task<IReadOnlyList<WarehouseDto>?> GetWarehousesAsync(string companyCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouses = await _context.Warehouses
            .Where(w => w.BusinessId == business.Id)
            .OrderBy(w => w.Name)
            .ToListAsync();

        var updated = false;
        foreach (var warehouse in warehouses)
        {
            if (string.IsNullOrWhiteSpace(warehouse.Cen))
            {
                warehouse.Cen = BuildCen("WH", warehouse.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return warehouses.Select(MapWarehouse).ToList();
    }

    public async Task<WarehouseDto?> CreateWarehouseAsync(string companyCen, CreateWarehouseRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Warehouse name is required.");

        var warehouse = new Warehouse
        {
            BusinessId = business.Id,
            Name = dto.Name.Trim(),
            IsActive = true
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        warehouse.Cen = BuildCen("WH", warehouse.Id);
        await _context.SaveChangesAsync();

        return MapWarehouse(warehouse);
    }

    public async Task<WarehouseDto?> UpdateWarehouseAsync(string companyCen, string warehouseCen, UpdateWarehouseRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, warehouseCen);
        if (warehouse == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            warehouse.Name = dto.Name.Trim();

        if (dto.IsActive.HasValue)
            warehouse.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return MapWarehouse(warehouse);
    }

    public async Task<IReadOnlyList<ProductDto>?> GetProductsAsync(
        string companyCen,
        string? search,
        string? categoryCen,
        string? status)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        int? categoryId = null;
        if (!string.IsNullOrWhiteSpace(categoryCen))
        {
            var category = await ResolveCategoryAsync(business.Id, categoryCen);
            if (category == null)
                return null;

            categoryId = category.Id;
        }

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.WarehouseProducts)
            .Where(p => p.BusinessId == business.Id)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(searchTerm)) ||
                (p.Sku != null && p.Sku.ToLower().Contains(searchTerm)) ||
                (p.Cen != null && p.Cen.ToLower().Contains(searchTerm)));
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync();

        var updated = false;
        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Cen))
            {
                product.Cen = string.IsNullOrWhiteSpace(product.Sku)
                    ? BuildCen("PROD", product.Id)
                    : product.Sku;
                updated = true;
            }

            if (product.Category != null && string.IsNullOrWhiteSpace(product.Category.Cen))
            {
                product.Category.Cen = BuildCen("CAT", product.Category.Id);
                updated = true;
            }

            if (product.Unit != null && string.IsNullOrWhiteSpace(product.Unit.Cen))
            {
                product.Unit.Cen = BuildCen("UNIT", product.Unit.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        var results = products.Select(p => MapProduct(p, GetProductStatus(p))).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToUpperInvariant();
            results = results.Where(p => p.Status == normalized).ToList();
        }

        return results;
    }

    public async Task<ProductDto?> GetProductAsync(string companyCen, string productCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<ProductDto?> CreateProductAsync(string companyCen, CreateProductRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Product name is required.");

        if (string.IsNullOrWhiteSpace(dto.Sku))
            throw new InvalidOperationException("SKU is required.");

        if (dto.SalePrice <= 0)
            throw new InvalidOperationException("Sale price must be greater than 0.");

        var category = await ResolveCategoryAsync(business.Id, dto.CategoryCen);
        if (category == null)
            throw new InvalidOperationException("Category not found for company.");

        var unit = await ResolveUnitAsync(business.Id, dto.UnitCen);
        if (unit == null)
            throw new InvalidOperationException("Unit not found for company.");

        var product = new Product
        {
            BusinessId = business.Id,
            Cen = dto.Sku.Trim(),
            Sku = dto.Sku.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            CategoryId = category.Id,
            UnitId = unit.Id,
            Price = dto.SalePrice,
            CostPrice = dto.CostPrice,
            ReorderLevel = dto.ReorderLevel,
            StationCode = dto.StationCode,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<ProductDto?> UpdateProductAsync(string companyCen, string productCen, UpdateProductRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            product.Name = dto.Name.Trim();

        if (dto.Description != null)
            product.Description = dto.Description;

        if (!string.IsNullOrWhiteSpace(dto.CategoryCen))
        {
            var category = await ResolveCategoryAsync(business.Id, dto.CategoryCen);
            if (category == null)
                throw new InvalidOperationException("Category not found for company.");

            product.CategoryId = category.Id;
        }

        if (!string.IsNullOrWhiteSpace(dto.UnitCen))
        {
            var unit = await ResolveUnitAsync(business.Id, dto.UnitCen);
            if (unit == null)
                throw new InvalidOperationException("Unit not found for company.");

            product.UnitId = unit.Id;
        }

        if (dto.SalePrice.HasValue)
        {
            if (dto.SalePrice.Value <= 0)
                throw new InvalidOperationException("Sale price must be greater than 0.");

            product.Price = dto.SalePrice.Value;
        }

        if (dto.CostPrice.HasValue)
            product.CostPrice = dto.CostPrice;

        if (dto.ReorderLevel.HasValue)
            product.ReorderLevel = dto.ReorderLevel.Value;

        if (dto.StationCode != null)
            product.StationCode = dto.StationCode;

        await _context.SaveChangesAsync();
        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<ProductDto?> UpdateProductStatusAsync(string companyCen, string productCen, UpdateProductStatusRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        var status = dto.Status.Trim().ToUpperInvariant();
        if (status is "ACTIVE")
        {
            product.IsActive = true;
            await SetWarehouseProductStatusAsync(product.Id, ActiveStatusId);
        }
        else if (status is "INACTIVE")
        {
            product.IsActive = false;
        }
        else if (status is "OUT_OF_STOCK")
        {
            product.IsActive = true;
            await SetWarehouseProductStatusAsync(product.Id, OutOfStockStatusId);
        }
        else
        {
            throw new InvalidOperationException("Invalid status value.");
        }

        await _context.SaveChangesAsync();
        return MapProduct(product, GetProductStatus(product));
    }

    public async Task<IReadOnlyList<StockItemDto>?> GetStockAsync(
        string companyCen,
        string? productCen,
        string? warehouseCen)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        int? productId = null;
        if (!string.IsNullOrWhiteSpace(productCen))
        {
            var product = await ResolveProductAsync(business.Id, productCen);
            if (product == null)
                return null;

            productId = product.Id;
        }

        int? warehouseId = null;
        if (!string.IsNullOrWhiteSpace(warehouseCen))
        {
            var warehouse = await ResolveWarehouseAsync(business.Id, warehouseCen);
            if (warehouse == null)
                return null;

            warehouseId = warehouse.Id;
        }

        var query = _context.WarehouseProducts
            .Include(wp => wp.Product)
                .ThenInclude(p => p!.Unit)
            .Include(wp => wp.Warehouse)
            .Where(wp => wp.Warehouse!.BusinessId == business.Id)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(wp => wp.ProductId == productId);

        if (warehouseId.HasValue)
            query = query.Where(wp => wp.WarehouseId == warehouseId);

        var items = await query.ToListAsync();

        var updated = false;
        foreach (var item in items)
        {
            if (item.Product != null && string.IsNullOrWhiteSpace(item.Product.Cen))
            {
                item.Product.Cen = string.IsNullOrWhiteSpace(item.Product.Sku)
                    ? BuildCen("PROD", item.Product.Id)
                    : item.Product.Sku;
                updated = true;
            }

            if (item.Warehouse != null && string.IsNullOrWhiteSpace(item.Warehouse.Cen))
            {
                item.Warehouse.Cen = BuildCen("WH", item.Warehouse.Id);
                updated = true;
            }
        }

        if (updated)
            await _context.SaveChangesAsync();

        return items.Select(item =>
        {
            var stockLeft = item.StockLeft ?? 0;
            var lowStockThreshold = item.LowStockQty ?? item.Product?.ReorderLevel ?? 0;
            return new StockItemDto
            {
                ProductCen = item.Product?.Cen ?? string.Empty,
                ProductName = item.Product?.Name ?? string.Empty,
                WarehouseCen = item.Warehouse?.Cen ?? string.Empty,
                WarehouseName = item.Warehouse?.Name ?? string.Empty,
                AvailableQuantity = stockLeft,
                ReservedQuantity = 0,
                UnitName = item.Product?.Unit?.Name ?? string.Empty,
                ReorderLevel = item.Product?.ReorderLevel ?? 0,
                IsLowStock = lowStockThreshold > 0 && stockLeft < lowStockThreshold
            };
        }).ToList();
    }

    public async Task<StockAdjustmentResponse?> CreateAdjustmentAsync(string companyCen, StockAdjustmentRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, dto.WarehouseCen);
        if (warehouse == null)
            return null;

        if (dto.Lines.Count == 0)
            throw new InvalidOperationException("At least one line is required.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var document = await CreateDocumentAsync(
            business.Id,
            warehouse.Id,
            "ADJUSTMENT",
            dto.Reason,
            null,
            "MANUAL_ADJUSTMENT",
            null,
            dto.Lines.Select(l => new InventoryDocumentLineRequest
            {
                ProductCen = l.ProductCen,
                Quantity = l.Quantity,
                UnitCost = null
            }).ToList());

        if (document == null)
            return null;

        var movements = new List<InventoryMovementDto>();

        foreach (var line in dto.Lines)
        {
            var product = await ResolveProductAsync(business.Id, line.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var adjustmentType = line.AdjustmentType.Trim().ToUpperInvariant();
            var currentStock = await GetCurrentStockAsync(warehouse.Id, product.Id);
            var qty = line.Quantity;
            decimal delta;
            string movementType;

            if (adjustmentType == "INCREASE")
            {
                delta = qty;
                movementType = "ADJUSTMENT_IN";
            }
            else if (adjustmentType == "DECREASE")
            {
                delta = -qty;
                movementType = "ADJUSTMENT_OUT";
            }
            else if (adjustmentType == "SET")
            {
                delta = qty - currentStock;
                movementType = delta >= 0 ? "ADJUSTMENT_IN" : "ADJUSTMENT_OUT";
            }
            else
            {
                throw new InvalidOperationException("Invalid adjustment type.");
            }

            if (delta == 0)
                continue;

            if (delta < 0 && currentStock + delta < 0)
                throw new InvalidOperationException("Cannot reduce stock below 0.");

            var movement = await ApplyStockChangeAsync(
                warehouse.Id,
                product,
                delta,
                movementType,
                dto.Reason,
                document.Id);

            movements.Add(movement);
        }

        await transaction.CommitAsync();

        return new StockAdjustmentResponse
        {
            AdjustmentCen = document.DocumentCen,
            Status = document.Status,
            GeneratedMovements = movements
        };
    }

    public async Task<IReadOnlyList<KardexMovementDto>?> GetKardexAsync(
        string companyCen,
        string productCen,
        string? warehouseCen,
        DateTime? from,
        DateTime? to)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var product = await ResolveProductAsync(business.Id, productCen);
        if (product == null)
            return null;

        int? warehouseId = null;
        if (!string.IsNullOrWhiteSpace(warehouseCen))
        {
            var warehouse = await ResolveWarehouseAsync(business.Id, warehouseCen);
            if (warehouse == null)
                return null;

            warehouseId = warehouse.Id;
        }

        var query = _context.Kardices
            .Include(k => k.Warehouse)
            .Include(k => k.Document)
            .Where(k => k.ProductId == product.Id)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(k => k.WarehouseId == warehouseId);

        if (from.HasValue)
            query = query.Where(k => k.TimeStamp >= from.Value);

        if (to.HasValue)
            query = query.Where(k => k.TimeStamp <= to.Value);

        var results = await query
            .OrderByDescending(k => k.TimeStamp)
            .ToListAsync();

        return results.Select(k => new KardexMovementDto
        {
            MovementCen = k.MovementCen ?? string.Empty,
            DocumentCen = k.Document?.DocumentCen,
            ProductCen = product.Cen ?? string.Empty,
            WarehouseCen = k.Warehouse?.Cen ?? string.Empty,
            MovementType = k.ActionType ?? string.Empty,
            Quantity = (decimal)(k.ActionQty ?? 0),
            UnitCost = null,
            Reason = k.Reason,
            CreatedAt = k.TimeStamp ?? DateTime.UtcNow
        }).ToList();
    }

    public async Task<InventoryDocumentDto?> CreateDocumentAsync(
        string companyCen,
        InventoryDocumentCreateRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, dto.WarehouseCen);
        if (warehouse == null)
            return null;

        if (dto.Lines.Count == 0)
            throw new InvalidOperationException("At least one line is required.");

        var normalizedType = dto.DocumentType.Trim().ToUpperInvariant();
        if (normalizedType is not ("ENTRY" or "EXIT" or "SALE_EXIT"))
            throw new InvalidOperationException("Invalid document type.");
        var requirements = await ValidateDocumentStockAsync(normalizedType, warehouse.Id, dto.Lines, business.Id);
        if (requirements.Count > 0)
            throw new InvalidOperationException("INSUFFICIENT_STOCK");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var document = await CreateDocumentAsync(
            business.Id,
            warehouse.Id,
            normalizedType,
            dto.Reason,
            dto.ExternalReference,
            dto.Source,
            dto.ReferenceCen,
            dto.Lines);

        if (document == null)
            return null;

        var movements = new List<string>();
        foreach (var line in dto.Lines)
        {
            var product = await ResolveProductAsync(business.Id, line.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var delta = GetDocumentDelta(document.DocumentType, line.Quantity);
            var movementType = delta >= 0 ? "ENTRY" : "EXIT";

            var movement = await ApplyStockChangeAsync(
                warehouse.Id,
                product,
                delta,
                movementType,
                dto.Reason,
                document.Id);

            movements.Add(movement.MovementCen);
        }

        await transaction.CommitAsync();

        return new InventoryDocumentDto
        {
            DocumentCen = document.DocumentCen,
            DocumentType = document.DocumentType,
            Status = document.Status,
            CreatedAt = document.CreatedAt,
            TotalItems = dto.Lines.Count,
            GeneratedMovementCens = movements
        };
    }

    public async Task<IReadOnlyList<InventoryDocumentDto>?> GetDocumentsAsync(
        string companyCen,
        string? documentType,
        DateTime? from,
        DateTime? to)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var query = _context.InventoryDocuments
            .Where(d => d.BusinessId == business.Id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            var normalized = documentType.Trim().ToUpperInvariant();
            query = query.Where(d => d.DocumentType == normalized);
        }
        else
        {
            query = query.Where(d => d.DocumentType == "ENTRY" || d.DocumentType == "EXIT" || d.DocumentType == "SALE_EXIT");
        }

        if (from.HasValue)
            query = query.Where(d => d.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(d => d.CreatedAt <= to.Value);

        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var documentIds = documents.Select(d => d.Id).ToList();
        var movementLookup = await _context.Kardices
            .Where(k => k.DocumentId.HasValue && documentIds.Contains(k.DocumentId.Value))
            .GroupBy(k => k.DocumentId!.Value)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(k => k.MovementCen ?? string.Empty).ToList());

        return documents.Select(d => new InventoryDocumentDto
        {
            DocumentCen = d.DocumentCen,
            DocumentType = d.DocumentType,
            Status = d.Status,
            CreatedAt = d.CreatedAt,
            TotalItems = _context.InventoryDocumentLines.Count(l => l.DocumentId == d.Id),
            GeneratedMovementCens = movementLookup.TryGetValue(d.Id, out var movements)
                ? movements
                : new List<string>()
        }).ToList();
    }

    public async Task<StockValidationResponse?> ValidateStockAsync(string companyCen, StockValidationRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, dto.WarehouseCen);
        if (warehouse == null)
            return null;

        var requirements = new List<StockRequirementDto>();

        foreach (var item in dto.Items)
        {
            var product = await ResolveProductAsync(business.Id, item.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var available = await GetCurrentStockAsync(warehouse.Id, product.Id);
            if (available < item.Quantity)
            {
                requirements.Add(new StockRequirementDto
                {
                    ProductCen = product.Cen ?? string.Empty,
                    ProductName = product.Name ?? string.Empty,
                    WarehouseCen = warehouse.Cen ?? string.Empty,
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = available,
                    MissingQuantity = item.Quantity - available,
                    UnitName = product.Unit?.Name ?? string.Empty,
                    Reason = "INSUFFICIENT_STOCK"
                });
            }
        }

        return new StockValidationResponse
        {
            IsValid = requirements.Count == 0,
            Requirements = requirements
        };
    }

    public async Task<StockConsumeResponse?> ConsumeStockAsync(string companyCen, StockConsumeRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, dto.WarehouseCen);
        if (warehouse == null)
            return null;

        var requirements = await ValidateDocumentStockAsync("SALE_EXIT", warehouse.Id, dto.Items.Select(i =>
            new InventoryDocumentLineRequest { ProductCen = i.ProductCen, Quantity = i.Quantity, UnitCost = null }).ToList(),
            business.Id);

        if (requirements.Count > 0)
        {
            return new StockConsumeResponse
            {
                Success = false,
                Message = "Stock insufficient to complete the sale.",
                Requirements = requirements
            };
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var document = await CreateDocumentAsync(
            business.Id,
            warehouse.Id,
            "SALE_EXIT",
            dto.Reason ?? "Sale payment",
            null,
            dto.Source,
            dto.ReferenceCen,
            dto.Items.Select(i => new InventoryDocumentLineRequest
            {
                ProductCen = i.ProductCen,
                Quantity = i.Quantity,
                UnitCost = null
            }).ToList());

        if (document == null)
            return null;

        var movements = new List<string>();
        foreach (var item in dto.Items)
        {
            var product = await ResolveProductAsync(business.Id, item.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var movement = await ApplyStockChangeAsync(
                warehouse.Id,
                product,
                -item.Quantity,
                "SALE_EXIT",
                dto.Reason ?? "Sale payment",
                document.Id);

            movements.Add(movement.MovementCen);
        }

        await transaction.CommitAsync();

        return new StockConsumeResponse
        {
            Success = true,
            DocumentCen = document.DocumentCen,
            DocumentType = document.DocumentType,
            GeneratedMovementCens = movements
        };
    }

    private async Task<InventoryDocument?> CreateDocumentAsync(
        int businessId,
        int warehouseId,
        string documentType,
        string? reason,
        string? externalReference,
        string? source,
        string? referenceCen,
        IReadOnlyList<InventoryDocumentLineRequest> lines)
    {
        var normalizedType = documentType.Trim().ToUpperInvariant();
        var document = new InventoryDocument
        {
            BusinessId = businessId,
            WarehouseId = warehouseId,
            DocumentType = normalizedType,
            Status = "REGISTERED",
            Reason = reason,
            ExternalReference = externalReference,
            Source = source,
            ReferenceCen = referenceCen,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryDocuments.Add(document);
        await _context.SaveChangesAsync();

        document.DocumentCen = BuildDocumentCen(normalizedType == "ADJUSTMENT" ? "ADJ" : "DOC", document.Id);
        await _context.SaveChangesAsync();

        foreach (var line in lines)
        {
            var product = await ResolveProductAsync(businessId, line.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            _context.InventoryDocumentLines.Add(new InventoryDocumentLine
            {
                DocumentId = document.Id,
                ProductId = product.Id,
                Quantity = (double)line.Quantity,
                UnitCost = line.UnitCost
            });
        }

        await _context.SaveChangesAsync();
        return document;
    }

    private async Task<IReadOnlyList<StockRequirementDto>> ValidateDocumentStockAsync(
        string documentType,
        int warehouseId,
        IReadOnlyList<InventoryDocumentLineRequest> lines,
        int businessId)
    {
        var requirements = new List<StockRequirementDto>();
        if (documentType is "ENTRY" or "ADJUSTMENT")
            return requirements;

        foreach (var line in lines)
        {
            var product = await ResolveProductAsync(businessId, line.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var available = await GetCurrentStockAsync(warehouseId, product.Id);
            if (available < line.Quantity)
            {
                var warehouse = await _context.Warehouses.FindAsync(warehouseId);
                requirements.Add(new StockRequirementDto
                {
                    ProductCen = product.Cen ?? string.Empty,
                    ProductName = product.Name ?? string.Empty,
                    WarehouseCen = warehouse?.Cen ?? string.Empty,
                    RequestedQuantity = line.Quantity,
                    AvailableQuantity = available,
                    MissingQuantity = line.Quantity - available,
                    UnitName = product.Unit?.Name ?? string.Empty,
                    Reason = "INSUFFICIENT_STOCK"
                });
            }
        }

        return requirements;
    }

    private static decimal GetDocumentDelta(string documentType, decimal quantity)
    {
        return documentType switch
        {
            "ENTRY" => quantity,
            "EXIT" => -quantity,
            "SALE_EXIT" => -quantity,
            "ADJUSTMENT" => quantity,
            _ => quantity
        };
    }

    private async Task<InventoryMovementDto> ApplyStockChangeAsync(
        int warehouseId,
        Product product,
        decimal delta,
        string movementType,
        string? reason,
        int documentId)
    {
        var warehouseProduct = await _context.WarehouseProducts
            .FirstOrDefaultAsync(wp => wp.WarehouseId == warehouseId && wp.ProductId == product.Id);

        if (warehouseProduct == null)
        {
            warehouseProduct = new WarehouseProduct
            {
                WarehouseId = warehouseId,
                ProductId = product.Id,
                StockLeft = 0,
                LowStockQty = product.ReorderLevel,
                StatusId = ActiveStatusId
            };
            _context.WarehouseProducts.Add(warehouseProduct);
            await _context.SaveChangesAsync();
        }

        var current = warehouseProduct.StockLeft ?? 0;
        var deltaInt = delta >= 0
            ? (int)Math.Ceiling(delta)
            : (int)Math.Floor(delta);
        var newStock = current + deltaInt;
        if (newStock < 0)
            throw new InvalidOperationException("Cannot reduce stock below 0.");

        warehouseProduct.StockLeft = newStock;
        warehouseProduct.StatusId = newStock == 0 ? OutOfStockStatusId : ActiveStatusId;

        var movementCen = BuildMovementCen();
        _context.Kardices.Add(new Kardex
        {
            WarehouseId = warehouseId,
            ProductId = product.Id,
            DocumentId = documentId,
            MovementCen = movementCen,
            ActionType = movementType,
            ActionQty = (double)Math.Abs(delta),
            Reason = reason,
            TimeStamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var warehouse = await _context.Warehouses.FindAsync(warehouseId);

        return new InventoryMovementDto
        {
            MovementCen = movementCen,
            ProductCen = product.Cen ?? string.Empty,
            WarehouseCen = warehouse?.Cen ?? string.Empty,
            Quantity = Math.Abs(delta),
            MovementType = movementType
        };
    }

    private async Task SetWarehouseProductStatusAsync(int productId, int statusId)
    {
        var items = await _context.WarehouseProducts
            .Where(wp => wp.ProductId == productId)
            .ToListAsync();

        foreach (var item in items)
            item.StatusId = statusId;
    }

    private async Task<Business?> ResolveBusinessAsync(string companyCen)
    {
        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Cen == companyCen);

        if (business == null && int.TryParse(companyCen, out var id))
            business = await _context.Businesses.FindAsync(id);

        if (business != null && string.IsNullOrWhiteSpace(business.Cen))
        {
            business.Cen = BuildCen("COMP", business.Id);
            await _context.SaveChangesAsync();
        }

        return business;
    }

    private async Task<Category?> ResolveCategoryAsync(int businessId, string categoryCen)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Cen == categoryCen);

        if (category == null && int.TryParse(categoryCen, out var id))
            category = await _context.Categories.FirstOrDefaultAsync(c => c.BusinessId == businessId && c.Id == id);

        if (category != null && string.IsNullOrWhiteSpace(category.Cen))
        {
            category.Cen = BuildCen("CAT", category.Id);
            await _context.SaveChangesAsync();
        }

        return category;
    }

    private async Task<Unit?> ResolveUnitAsync(int businessId, string unitCen)
    {
        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.BusinessId == businessId && u.Cen == unitCen);

        if (unit == null && int.TryParse(unitCen, out var id))
            unit = await _context.Units.FirstOrDefaultAsync(u => u.BusinessId == businessId && u.Id == id);

        if (unit != null && string.IsNullOrWhiteSpace(unit.Cen))
        {
            unit.Cen = BuildCen("UNIT", unit.Id);
            await _context.SaveChangesAsync();
        }

        return unit;
    }

    private async Task<Warehouse?> ResolveWarehouseAsync(int businessId, string warehouseCen)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.BusinessId == businessId && w.Cen == warehouseCen);

        if (warehouse == null && int.TryParse(warehouseCen, out var id))
            warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.BusinessId == businessId && w.Id == id);

        if (warehouse != null && string.IsNullOrWhiteSpace(warehouse.Cen))
        {
            warehouse.Cen = BuildCen("WH", warehouse.Id);
            await _context.SaveChangesAsync();
        }

        return warehouse;
    }

    private async Task<Product?> ResolveProductAsync(int businessId, string productCen)
    {
        var product = await _context.Products
            .Include(p => p.Unit)
            .Include(p => p.Category)
            .Include(p => p.WarehouseProducts)
            .FirstOrDefaultAsync(p => p.BusinessId == businessId && (p.Cen == productCen || p.Sku == productCen));

        if (product == null && int.TryParse(productCen, out var id))
            product = await _context.Products
                .Include(p => p.Unit)
                .Include(p => p.Category)
                .Include(p => p.WarehouseProducts)
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.Id == id);

        var updated = false;
        if (product != null && string.IsNullOrWhiteSpace(product.Cen))
        {
            product.Cen = string.IsNullOrWhiteSpace(product.Sku)
                ? BuildCen("PROD", product.Id)
                : product.Sku;
            updated = true;
        }

        if (product != null && product.Category != null && string.IsNullOrWhiteSpace(product.Category.Cen))
        {
            product.Category.Cen = BuildCen("CAT", product.Category.Id);
            updated = true;
        }

        if (product != null && product.Unit != null && string.IsNullOrWhiteSpace(product.Unit.Cen))
        {
            product.Unit.Cen = BuildCen("UNIT", product.Unit.Id);
            updated = true;
        }

        if (product != null && updated)
            await _context.SaveChangesAsync();

        return product;
    }

    private async Task<decimal> GetCurrentStockAsync(int warehouseId, int productId)
    {
        var stock = await _context.WarehouseProducts
            .Where(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId)
            .Select(wp => wp.StockLeft ?? 0)
            .FirstOrDefaultAsync();

        return stock;
    }

    private static string BuildCen(string prefix, int id)
    {
        return $"{prefix}-{id:D6}";
    }

    private static string BuildDocumentCen(string prefix, int id)
    {
        return $"{prefix}-{id:D6}";
    }

    private static string BuildMovementCen()
    {
        return $"MOV-{Guid.NewGuid():N}";
    }

    private static CategoryDto MapCategory(Category category)
    {
        return new CategoryDto
        {
            CategoryCen = category.Cen ?? BuildCen("CAT", category.Id),
            Name = category.Name ?? string.Empty,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }

    private static UnitDto MapUnit(Unit unit)
    {
        return new UnitDto
        {
            UnitCen = unit.Cen ?? BuildCen("UNIT", unit.Id),
            Name = unit.Name ?? string.Empty,
            Abbreviation = unit.Abbreviation,
            IsActive = unit.IsActive
        };
    }

    private static WarehouseDto MapWarehouse(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            WarehouseCen = warehouse.Cen ?? BuildCen("WH", warehouse.Id),
            Name = warehouse.Name ?? string.Empty,
            IsActive = warehouse.IsActive
        };
    }

    private static ProductDto MapProduct(Product product, string status)
    {
        return new ProductDto
        {
            ProductCen = product.Cen ?? BuildCen("PROD", product.Id),
            Sku = product.Sku ?? product.Cen ?? BuildCen("PROD", product.Id),
            Name = product.Name ?? string.Empty,
            Description = product.Description,
            CategoryCen = product.Category?.Cen ?? string.Empty,
            CategoryName = product.Category?.Name ?? string.Empty,
            UnitCen = product.Unit?.Cen ?? string.Empty,
            UnitName = product.Unit?.Name ?? string.Empty,
            SalePrice = product.Price ?? 0,
            CostPrice = product.CostPrice,
            ReorderLevel = product.ReorderLevel,
            Status = status,
            StationCode = product.StationCode
        };
    }

    private static string GetProductStatus(Product product)
    {
        if (!product.IsActive.GetValueOrDefault(true))
            return "INACTIVE";

        var totalStock = product.WarehouseProducts.Sum(wp => wp.StockLeft ?? 0);
        if (totalStock <= 0)
            return "OUT_OF_STOCK";

        return "ACTIVE";
    }
}
