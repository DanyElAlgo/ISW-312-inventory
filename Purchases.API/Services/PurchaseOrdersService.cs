using Microsoft.Extensions.Options;
using Purchases.API.DTOs;
using Purchases.API.Exceptions;
using Purchases.API.HttpClients;
using Purchases.API.Models;
using Purchases.API.Repositories.Interfaces;
using Purchases.Domain.Enums;

namespace Purchases.API.Services;

public class PurchaseOrdersService
{
    private readonly ISupplierRepository _suppliers;
    private readonly IPurchaseOrderRepository _orders;
    private readonly IPurchaseOrderItemRepository _items;
    private readonly PurchaseStatusesService _statuses;
    private readonly InventoryClient _inventory;
    private readonly InventoryIntegrationOptions _integration;
    private readonly IPurchasesUnitOfWork _uow;

    public PurchaseOrdersService(
        ISupplierRepository suppliers,
        IPurchaseOrderRepository orders,
        IPurchaseOrderItemRepository items,
        PurchaseStatusesService statuses,
        InventoryClient inventory,
        IOptions<InventoryIntegrationOptions> integrationOptions,
        IPurchasesUnitOfWork uow)
    {
        _suppliers = suppliers;
        _orders = orders;
        _items = items;
        _statuses = statuses;
        _inventory = inventory;
        _integration = integrationOptions.Value;
        _uow = uow;
    }

    public async Task<PagedResultDto<PurchaseOrderListDto>?> ListAsync(
        string companyCen,
        PurchaseStatusEnum? externalStatus,
        int page,
        int pageSize,
        bool sortDescending)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 200) pageSize = 200;

        int? statusId = null;
        if (externalStatus.HasValue)
            statusId = await _statuses.FromExternalAsync((int)externalStatus.Value)
                ?? throw new InvalidOperationException(
                    $"Unknown PurchaseStatus value '{externalStatus.Value}'. Allowed: Pending, Confirmed, Cancelled.");

        var (rows, totalCount) = await _orders.SearchAsync(companyCen, statusId, page, pageSize, sortDescending);

        var items = new List<PurchaseOrderListDto>();
        foreach (var row in rows)
            items.Add(await MapListAsync(row));

        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResultDto<PurchaseOrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
        };
    }

    public async Task<PurchaseOrderDetailDto?> GetAsync(string companyCen, string orderCen)
    {
        var order = await _orders.GetByCenAsync(companyCen, orderCen, includeItems: true);
        if (order == null) return null;

        return await MapDetailAsync(order);
    }

    public async Task<PurchaseOrderSummaryDto?> CreateAsync(string companyCen, CreatePurchaseOrderDto request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("items must contain at least one product.");
        if (request.Items.Any(i => i.Quantity <= 0))
            throw new InvalidOperationException("Each item quantity must be greater than 0.");
        if (request.Items.Any(i => string.IsNullOrWhiteSpace(i.ProductCen)))
            throw new InvalidOperationException("Each item must include productCen.");

        var supplier = await _suppliers.GetByCenAsync(companyCen, request.SupplierCen);
        if (supplier == null || !supplier.IsActive)
            throw new InvalidOperationException($"Supplier '{request.SupplierCen}' not found for this company.");

        var pendingId = await _statuses.GetPendingIdAsync();

        var order = _orders.Add(new PurchaseOrder
        {
            CompanyCen = companyCen,
            SupplierId = supplier.Id,
            WarehouseCen = request.WarehouseCen.Trim(),
            StatusId = pendingId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        });
        await _uow.SaveChangesAsync();

        order.Cen = $"PO-{order.Id:D6}";

        foreach (var line in request.Items)
        {
            string? productName = null;
            try
            {
                var product = await _inventory.GetProductAsync(companyCen, line.ProductCen);
                productName = product?.Name;
            }
            catch (Exception ex) when (ex is InvalidOperationException or InventoryUnavailableException) { }

            _items.Add(new PurchaseOrderItem
            {
                PurchaseOrderId = order.Id,
                ProductCen = line.ProductCen.Trim(),
                ProductName = productName,
                Quantity = line.Quantity,
            });
        }

        await _uow.SaveChangesAsync();

        return new PurchaseOrderSummaryDto
        {
            OrderCen = order.Cen!,
            Status = PurchaseStatusEnum.Pending,
        };
    }

    public async Task<PurchaseOrderConfirmationDto?> ConfirmAsync(string companyCen, string orderCen)
    {
        var order = await _orders.GetByCenAsync(companyCen, orderCen, includeItems: true);
        if (order == null) return null;

        var pendingId = await _statuses.GetPendingIdAsync();
        var confirmedId = await _statuses.GetConfirmedIdAsync();
        if (order.StatusId != pendingId)
            throw new InvalidOperationException("Only orders in Pending status can be confirmed.");

        if (order.Items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an order with no items.");

        var confirmedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        await using var tx = await _uow.BeginTransactionAsync();

        order.StatusId = confirmedId;
        order.ConfirmedAt = confirmedAt;
        await _uow.SaveChangesAsync();

        var stockRequest = new StockIncreaseRequest
        {
            WarehouseCen = order.WarehouseCen,
            Source = _integration.Source,
            ReferenceCen = order.Cen,
            Reason = $"Purchase order {order.Cen} confirmed",
            Items = order.Items
                .Select(i => new StockIncreaseItemDto
                {
                    ProductCen = i.ProductCen,
                    Quantity = i.Quantity,
                })
                .ToList(),
        };

        var stockResponse = await _inventory.IncreaseStockAsync(companyCen, stockRequest);
        if (stockResponse != null)
        {
            order.InventoryDocumentCen = stockResponse.DocumentCen;
            await _uow.SaveChangesAsync();
        }

        await tx.CommitAsync();

        return new PurchaseOrderConfirmationDto
        {
            OrderCen = order.Cen!,
            Status = PurchaseStatusEnum.Confirmed,
            ConfirmedAt = DateTime.SpecifyKind(confirmedAt, DateTimeKind.Utc),
        };
    }

    private async Task<PurchaseOrderListDto> MapListAsync(PurchaseOrder order)
    {
        return new PurchaseOrderListDto
        {
            OrderCen = order.Cen ?? $"PO-{order.Id:D6}",
            Status = (PurchaseStatusEnum)order.StatusId,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            SupplierCen = order.Supplier?.Cen ?? string.Empty,
            ItemCount = order.Items.Count,
        };
    }

    private async Task<PurchaseOrderDetailDto> MapDetailAsync(PurchaseOrder order)
    {
        return new PurchaseOrderDetailDto
        {
            OrderCen = order.Cen ?? $"PO-{order.Id:D6}",
            Status = (PurchaseStatusEnum)order.StatusId,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            SupplierCen = order.Supplier?.Cen ?? string.Empty,
            WarehouseCen = order.WarehouseCen,
            Items = order.Items
                .Select(i => new PurchaseOrderDetailItemDto
                {
                    ProductCen = i.ProductCen,
                    Quantity = i.Quantity,
                })
                .ToList(),
        };
    }
}
