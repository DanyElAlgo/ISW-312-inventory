using Sales.API.DTOs;
using Sales.API.Helpers;
using Sales.API.HttpClients;
using Sales.API.Models;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class OrderItemsService
{
    private readonly IOrderItemRepository _items;
    private readonly IOrderTicketRepository _tickets;
    private readonly ICommandItemRepository _commandItems;
    private readonly InventoryClient _inventoryClient;
    private readonly OrderStatusesService _statuses;
    private readonly ISalesUnitOfWork _uow;

    public OrderItemsService(
        IOrderItemRepository items,
        IOrderTicketRepository tickets,
        ICommandItemRepository commandItems,
        InventoryClient inventoryClient,
        OrderStatusesService statuses,
        ISalesUnitOfWork uow)
    {
        _items = items;
        _tickets = tickets;
        _commandItems = commandItems;
        _inventoryClient = inventoryClient;
        _statuses = statuses;
        _uow = uow;
    }

    public async Task<IReadOnlyList<TicketItemContractResponse>?> GetItemsAsync(string ticketCen)
    {
        var ticket = await _tickets.GetByCenAsync(ticketCen);
        if (ticket == null)
            return null;

        var items = await _items.GetByOrderIdAsync(ticket.Id, includeStatus: true);
        return items.Select(OrderItemMapping.MapToContract).ToList();
    }

    public async Task<TicketItemContractResponse?> AddItemAsync(string ticketCen, CreateTicketItemContractRequest request)
    {
        var ticket = await _tickets.GetByCenAsync(ticketCen, includeStatus: true);
        if (ticket == null)
            return null;

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is not ("open" or "abierto"))
            throw new InvalidOperationException("Ticket is not open.");

        if (string.IsNullOrWhiteSpace(ticket.CompanyCen))
            throw new InvalidOperationException("Ticket has no company assigned.");

        var product = await _inventoryClient.GetProductAsync(ticket.CompanyCen, request.ProductCen);
        if (product == null)
            throw new InvalidOperationException("Product not found.");

        var status = product.Status.Trim().ToUpperInvariant();
        if (status is "INACTIVE")
            throw new InvalidOperationException("Product is inactive.");
        if (status is "OUT_OF_STOCK")
            throw new InvalidOperationException("Product is out of stock.");

        var pendingStatusId = await _statuses.GetPendingStatusIdAsync();

        var item = _items.Add(new OrderItem
        {
            OrderId = ticket.Id,
            ProductCen = product.ProductCen,
            ProductName = product.Name,
            UnitPrice = product.SalePrice,
            Qty = request.Quantity,
            AdditionalNote = request.Note,
            StatusId = pendingStatusId,
            ResendCount = 0
        });
        await _uow.SaveChangesAsync();

        item.Cen = SalesCenBuilder.BuildItemCen(item.Id);
        await _uow.SaveChangesAsync();

        var saved = await _items.GetByIdAsync(item.Id, includeStatus: true);
        return saved == null ? null : OrderItemMapping.MapToContract(saved);
    }

    public async Task<TicketItemContractResponse?> UpdateItemAsync(
        string ticketCen,
        string ticketItemCen,
        UpdateTicketItemContractRequest request)
    {
        var ticket = await _tickets.GetByCenAsync(ticketCen);
        if (ticket == null)
            return null;

        var item = await _items.GetByCenAsync(ticketItemCen, includeStatus: true);
        if (item == null || item.OrderId != ticket.Id)
            return null;

        if (request.Quantity.HasValue)
            item.Qty = request.Quantity.Value;

        if (request.Note != null)
            item.AdditionalNote = request.Note;

        await _uow.SaveChangesAsync();
        return OrderItemMapping.MapToContract(item);
    }

    public async Task<TicketItemContractResponse?> ResendItemAsync(string ticketCen, string ticketItemCen)
    {
        var ticket = await _tickets.GetByCenAsync(ticketCen);
        if (ticket == null)
            return null;

        var item = await _items.GetByCenAsync(ticketItemCen, includeStatus: true);
        if (item == null || item.OrderId != ticket.Id)
            return null;

        var pendingStatusId = await _statuses.GetPendingStatusIdAsync();
        item.StatusId = pendingStatusId;
        item.ResendCount++;
        item.SentAt = null;

        await _commandItems.RemoveByOrderItemIdAsync(item.Id);
        await _uow.SaveChangesAsync();

        var updated = await _items.GetByIdAsync(item.Id, includeStatus: true);
        return updated == null ? null : OrderItemMapping.MapToContract(updated);
    }
}
