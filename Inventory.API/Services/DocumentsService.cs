using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Services;

public class DocumentsService : InventoryServiceBase
{
    public DocumentsService(
        InventoryDbContext context,
        IBusinessRepository businessRepository,
        ICategoryRepository categoryRepository,
        IUnitRepository unitRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseProductRepository warehouseProductRepository)
        : base(context, businessRepository, categoryRepository, unitRepository, warehouseRepository, productRepository, warehouseProductRepository)
    {
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

        await using var transaction = await Context.Database.BeginTransactionAsync();

        var document = await CreateDocumentInternalAsync(
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

        var query = Context.InventoryDocuments
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
        var movementLookup = await Context.Kardices
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
            TotalItems = Context.InventoryDocumentLines.Count(l => l.DocumentId == d.Id),
            GeneratedMovementCens = movementLookup.TryGetValue(d.Id, out var movements)
                ? movements
                : new List<string>()
        }).ToList();
    }

    private async Task<InventoryDocument?> CreateDocumentInternalAsync(
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
                var warehouse = await Context.Warehouses.FindAsync(warehouseId);
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
