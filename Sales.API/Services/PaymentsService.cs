using Microsoft.Extensions.Options;
using Sales.API.DTOs;
using Sales.API.HttpClients;
using Sales.API.Models;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class PaymentsService
{
    private readonly IOrderTicketRepository _tickets;
    private readonly IPaymentRepository _payments;
    private readonly IPaymentTypeRepository _paymentTypes;
    private readonly InventoryClient _inventoryClient;
    private readonly InventoryIntegrationOptions _integrationOptions;
    private readonly OrderTicketsService _orderTicketsService;
    private readonly OrderStatusesService _statuses;
    private readonly ISalesUnitOfWork _uow;

    public PaymentsService(
        IOrderTicketRepository tickets,
        IPaymentRepository payments,
        IPaymentTypeRepository paymentTypes,
        InventoryClient inventoryClient,
        IOptions<InventoryIntegrationOptions> integrationOptions,
        OrderTicketsService orderTicketsService,
        OrderStatusesService statuses,
        ISalesUnitOfWork uow)
    {
        _tickets = tickets;
        _payments = payments;
        _paymentTypes = paymentTypes;
        _inventoryClient = inventoryClient;
        _integrationOptions = integrationOptions.Value;
        _orderTicketsService = orderTicketsService;
        _statuses = statuses;
        _uow = uow;
    }

    public async Task<(PayTicketContractResponse? success, ProcessRestaurantOrderPaymentResultDto? conflict)>
        PayTicketAsync(string companyCen, string ticketCen, string paymentMethodCode)
    {
        if (!int.TryParse(ticketCen, out var ticketId))
            throw new InvalidOperationException("Invalid ticketCen.");

        var ticket = await _tickets.GetByIdAsync(ticketId, includeItems: true, includeStatus: true);
        if (ticket == null)
            throw new InvalidOperationException("Ticket not found.");

        var statusName = ticket.Status?.Name?.ToLower() ?? "";
        if (statusName is not ("open" or "abierto"))
            throw new InvalidOperationException("Ticket is not open.");

        var waiter = await _orderTicketsService.GetAssignedWaiterAsync(ticketId);
        if (waiter == null)
            throw new InvalidOperationException("A waiter must be assigned before payment.");

        var items = ticket.OrderItems.Where(i => !string.IsNullOrWhiteSpace(i.ProductCen)).ToList();
        if (!items.Any())
            throw new InvalidOperationException("Ticket has no items.");

        var paymentType = await _paymentTypes.FindByCodeOrNameAsync(paymentMethodCode);
        if (paymentType == null)
            throw new InvalidOperationException($"Payment method '{paymentMethodCode}' not found.");

        var validateDto = new StockValidationRequest
        {
            WarehouseCen = _integrationOptions.WarehouseCen,
            Source = _integrationOptions.Source,
            ReferenceCen = BuildAccountNumber(ticketId),
            Items = items.Select(i => new StockValidationItemDto
            {
                ProductCen = i.ProductCen!,
                Quantity = (decimal)(i.Qty ?? 0)
            }).ToList()
        };

        var validation = await _inventoryClient.ValidateStockAsync(_integrationOptions.CompanyCen, validateDto);
        if (validation == null)
            throw new InvalidOperationException("Could not validate stock.");

        if (!validation.IsValid)
        {
            var subtotal = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
            var tax = subtotal * (ticket.TaxRateSnapshot ?? 0);
            var conflict = new ProcessRestaurantOrderPaymentResultDto
            {
                IsSuccess = false,
                Message = "Insufficient stock.",
                Subtotal = subtotal,
                TaxAmount = tax,
                Total = subtotal + tax,
                Insufficiencies = validation.Requirements.Select(r => new StockInsufficiencyResponseDto
                {
                    ProductCen = r.ProductCen,
                    ProductName = r.ProductName,
                    WarehouseCen = r.WarehouseCen,
                    RequestedQuantity = (int)r.RequestedQuantity,
                    AvailableQuantity = (int)r.AvailableQuantity,
                    MissingQuantity = (int)r.MissingQuantity
                }).ToList()
            };
            return (null, conflict);
        }

        var consumeDto = new StockConsumeRequest
        {
            WarehouseCen = _integrationOptions.WarehouseCen,
            Source = _integrationOptions.Source,
            ReferenceCen = BuildAccountNumber(ticketId),
            Reason = $"Sale — ticket #{ticketId}",
            Items = items.Select(i => new StockConsumeItemDto
            {
                ProductCen = i.ProductCen!,
                Quantity = (decimal)(i.Qty ?? 0)
            }).ToList()
        };

        var deduction = await _inventoryClient.ConsumeStockAsync(_integrationOptions.CompanyCen, consumeDto);
        if (deduction == null || !deduction.Success)
            throw new InvalidOperationException(deduction?.Message ?? "Stock deduction failed.");

        var payment = _payments.Add(new Payment
        {
            OrderId = ticketId,
            PaymentTypeId = paymentType.Id,
            PaidAt = DateTime.UtcNow
        });
        ticket.StatusId = await _statuses.GetPaidStatusIdAsync();
        await _uow.SaveChangesAsync();

        var sub = items.Sum(i => (i.UnitPrice ?? 0) * (decimal)(i.Qty ?? 0));
        var taxAmount = sub * (ticket.TaxRateSnapshot ?? 0);

        return (new PayTicketContractResponse
        {
            SaleCen = $"SALE-{payment.Id}",
            TicketCen = ticketId.ToString(),
            Status = "Pagado",
            Subtotal = sub,
            TaxAmount = taxAmount,
            Total = sub + taxAmount,
            InventoryDocumentCen = deduction.DocumentCen
        }, null);
    }

    private static string BuildAccountNumber(int ticketId) => $"ACC-{ticketId:D6}";
}
