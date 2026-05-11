using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Interfaces;
using Inventory.API.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Services;

public class StockConsumeService : InventoryServiceBase
{
    public StockConsumeService(
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

        await using var transaction = await Context.Database.BeginTransactionAsync();

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
            TimeStamp = DateTime.UtcNow
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
