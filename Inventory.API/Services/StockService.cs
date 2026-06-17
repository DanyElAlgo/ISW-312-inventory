using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Services;

public class StockService : InventoryServiceBase
{
    private readonly RestockNotifier _restockNotifier;

    public StockService(
        InventoryDbContext context,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitRepository unitRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseProductRepository warehouseProductRepository,
        RestockNotifier restockNotifier)
        : base(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository)
    {
        _restockNotifier = restockNotifier;
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

        var items = await WarehouseProductRepository.GetByBusinessIdAsync(business.Id, productId, warehouseId);

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

    public async Task<StockIncreaseResponse?> IncreaseStockAsync(string companyCen, StockIncreaseRequest dto)
    {
        var business = await ResolveBusinessAsync(companyCen);
        if (business == null)
            return null;

        var warehouse = await ResolveWarehouseAsync(business.Id, dto.WarehouseCen);
        if (warehouse == null)
            return null;

        if (dto.Items.Count == 0)
            throw new InvalidOperationException("At least one item is required.");

        await using var transaction = await Context.Database.BeginTransactionAsync();

        var document = await CreateDocumentAsync(
            business.Id,
            warehouse.Id,
            "ENTRY",
            dto.Reason ?? "Stock increase",
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
        var restockEvents = new List<RestockEvent>();
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than 0.");

            var product = await ResolveProductAsync(business.Id, item.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            var movement = await ApplyStockChangeAsync(
                warehouse.Id,
                product,
                item.Quantity,
                "ENTRY",
                dto.Reason ?? "Stock increase",
                document.Id);

            movements.Add(movement.MovementCen);
            restockEvents.Add(new RestockEvent
            {
                CompanyCen = companyCen,
                ProductCen = product.Cen ?? item.ProductCen,
                ProductName = product.Name ?? string.Empty,
                Quantity = item.Quantity,
                WarehouseCen = dto.WarehouseCen,
                OccurredAt = DateTime.UtcNow,
            });
        }

        await transaction.CommitAsync();

        foreach (var evt in restockEvents)
            _restockNotifier.Publish(evt);

        return new StockIncreaseResponse
        {
            DocumentCen = document.DocumentCen,
            DocumentType = document.DocumentType,
            GeneratedMovementCens = movements
        };
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

        await using var transaction = await Context.Database.BeginTransactionAsync();

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
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        Context.InventoryDocuments.Add(document);
        await Context.SaveChangesAsync();

        document.DocumentCen = BuildDocumentCen(normalizedType == "ADJUSTMENT" ? "ADJ" : "DOC", document.Id);
        await Context.SaveChangesAsync();

        foreach (var line in lines)
        {
            var product = await ResolveProductAsync(businessId, line.ProductCen);
            if (product == null)
                throw new InvalidOperationException("Product not found for company.");

            Context.InventoryDocumentLines.Add(new InventoryDocumentLine
            {
                DocumentId = document.Id,
                ProductId = product.Id,
                Quantity = (double)line.Quantity,
                UnitCost = line.UnitCost
            });
        }

        await Context.SaveChangesAsync();
        return document;
    }

    private async Task<InventoryMovementDto> ApplyStockChangeAsync(
        int warehouseId,
        Product product,
        decimal delta,
        string movementType,
        string? reason,
        int documentId)
    {
        var warehouseProduct = await Context.WarehouseProducts
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
            Context.WarehouseProducts.Add(warehouseProduct);
            await Context.SaveChangesAsync();
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
        Context.Kardices.Add(new Kardex
        {
            WarehouseId = warehouseId,
            ProductId = product.Id,
            DocumentId = documentId,
            MovementCen = movementCen,
            ActionType = movementType,
            ActionQty = (double)Math.Abs(delta),
            Reason = reason,
            TimeStamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        });

        await Context.SaveChangesAsync();

        var warehouse = await Context.Warehouses.FindAsync(warehouseId);

        return new InventoryMovementDto
        {
            MovementCen = movementCen,
            ProductCen = product.Cen ?? string.Empty,
            WarehouseCen = warehouse?.Cen ?? string.Empty,
            Quantity = Math.Abs(delta),
            MovementType = movementType
        };
    }
}
