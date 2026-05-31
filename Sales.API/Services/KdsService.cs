using Microsoft.Extensions.Options;
using Sales.API.DTOs;
using Sales.API.HttpClients;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class KdsService
{
    private readonly IStationTypeRepository _stationTypes;
    private readonly IStationRepository _stations;
    private readonly ICommandItemRepository _commandItems;
    private readonly IOrderItemRepository _orderItems;
    private readonly InventoryClient _inventoryClient;
    private readonly InventoryIntegrationOptions _integrationOptions;
    private readonly OrderStatusesService _statuses;
    private readonly ISalesUnitOfWork _uow;

    public KdsService(
        IStationTypeRepository stationTypes,
        IStationRepository stations,
        ICommandItemRepository commandItems,
        IOrderItemRepository orderItems,
        InventoryClient inventoryClient,
        IOptions<InventoryIntegrationOptions> integrationOptions,
        OrderStatusesService statuses,
        ISalesUnitOfWork uow)
    {
        _stationTypes = stationTypes;
        _stations = stations;
        _commandItems = commandItems;
        _orderItems = orderItems;
        _inventoryClient = inventoryClient;
        _integrationOptions = integrationOptions.Value;
        _statuses = statuses;
        _uow = uow;
    }

    public async Task<IReadOnlyList<KdsTeamContractResponse>> GetKdsTeamsAsync()
    {
        var types = await _stationTypes.GetAllWithStationsAsync();
        var result = new List<KdsTeamContractResponse>();
        foreach (var t in types)
        {
            var categoryIds = await _stationTypes.GetCategoryIdsAsync(t.Id);
            result.Add(new KdsTeamContractResponse
            {
                TeamCen = t.Id.ToString(),
                Name = t.Name ?? string.Empty,
                CategoryCens = categoryIds.Select(id => id.ToString()).ToList()
            });
        }
        return result;
    }

    public async Task<IReadOnlyList<KdsItemContractResponse>?> GetKdsItemsByTeamAsync(string teamCen)
    {
        if (!int.TryParse(teamCen, out var stationTypeId))
            return null;

        var type = await _stationTypes.GetByIdAsync(stationTypeId);
        if (type == null)
            return null;

        var commandItems = await _commandItems.GetByStationTypeIdAsync(stationTypeId);

        return commandItems.Select(ci => new KdsItemContractResponse
        {
            TicketItemCen = ci.OrderItem!.Cen ?? ci.OrderItem.Id.ToString(),
            TicketCen = ci.OrderItem.Order?.Cen ?? (ci.OrderItem.OrderId ?? 0).ToString(),
            ProductCen = ci.OrderItem.ProductCen ?? string.Empty,
            ProductName = ci.OrderItem.ProductName ?? string.Empty,
            Quantity = (int)(ci.OrderItem.Qty ?? 0),
            Status = ci.OrderItem.Status?.Name ?? "Pending",
            Note = ci.OrderItem.AdditionalNote,
            ResendCount = ci.OrderItem.ResendCount,
            CreatedAt = ci.OrderItem.SentAt.HasValue ? ci.OrderItem.SentAt.Value.ToString("O") : string.Empty
        }).ToList();
    }

    public async Task<bool?> UpdateItemStatusAsync(string ticketItemCen, string newStatus)
    {
        var item = await _orderItems.GetByCenAsync(ticketItemCen, includeStatus: true);
        if (item == null)
            return null;

        var normalized = newStatus.Trim().ToLower();
        int targetStatusId = normalized switch
        {
            "created" or "pending" or "pendiente" => await _statuses.GetPendingStatusIdAsync(),
            "preparing" or "en preparacion" or "en preparación" => await _statuses.GetInPreparationStatusIdAsync(),
            "delivered" or "ready" or "listo" => await _statuses.GetReadyStatusIdAsync(),
            "canceled" or "cancelado" => await _statuses.GetCancelledStatusIdAsync(),
            _ => throw new InvalidOperationException($"Unknown KDS status '{newStatus}'. Valid values: created, preparing, delivered, canceled.")
        };

        item.StatusId = targetStatusId;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<int?> ResolveStationForProductAsync(string productCen)
    {
        var product = await _inventoryClient.GetProductAsync(_integrationOptions.CompanyCen, productCen);
        if (product == null || string.IsNullOrWhiteSpace(product.StationCode))
            return null;

        var normalized = product.StationCode.Trim().ToLower();

        var stationTypeId = await _stationTypes.FindIdByLowercaseNameAsync(normalized);
        if (stationTypeId.HasValue && stationTypeId.Value != 0)
        {
            var stations = await _stations.GetByTypeIdAsync(stationTypeId.Value);
            var first = stations.FirstOrDefault();
            return first?.Id;
        }

        var station = await _stations.FindByLowercaseNameAsync(normalized);
        return station?.Id;
    }
}
