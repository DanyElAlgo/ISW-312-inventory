namespace Sales.API.DTOs;

// ── Catalog ──────────────────────────────────────────────────────────────────

public class SellableProductContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public double AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? StationCode { get; set; }
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

public class DailySalesDashboardDto
{
    public decimal TotalSales { get; set; }
    public int TicketsCount { get; set; }
    public decimal AverageTicket { get; set; }
}

public class TopProductDashboardContractResponse
{
    public string? ProductCen { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public string? CategoryCen { get; set; }
    public string? CategoryName { get; set; }
    public decimal SalePrice { get; set; }
}

public class KdsStatusDashboardDto
{
    public int PendingCount { get; set; }
    public int PreparingCount { get; set; }
    public int ReadyCount { get; set; }
}

// ── KDS ───────────────────────────────────────────────────────────────────────

public class KdsTeamContractResponse
{
    public string TeamCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> CategoryCens { get; set; } = new();
}

public class KdsItemContractResponse
{
    public string TicketItemCen { get; set; } = string.Empty;
    public string TicketCen { get; set; } = string.Empty;
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int ResendCount { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class UpdateKdsItemStatusContractRequest
{
    public string Status { get; set; } = string.Empty;
}

// ── Payment Methods ───────────────────────────────────────────────────────────

public class PaymentMethodContractResponse
{
    public string PaymentMethodCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ── Tax Configuration ─────────────────────────────────────────────────────────

public class TaxConfigurationContractResponse
{
    public string CompanyCen { get; set; } = string.Empty;
    public decimal GlobalTaxPercentage { get; set; }
}

public class UpdateTaxConfigurationContractRequest
{
    public decimal GlobalTaxPercentage { get; set; }
}

// ── Tickets ───────────────────────────────────────────────────────────────────

public class TicketContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public int DailyNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? WaiterCen { get; set; }
    public string? CompanyCen { get; set; }
    public string? WarehouseCen { get; set; }
    public decimal TaxAmount { get; set; }
}

public class CreateTicketContractRequest
{
    public string? WaiterCen { get; set; }

    // Optional: when omitted, Sales resolves a default warehouse for the company
    // (configured default → first active warehouse) so contract-only clients work.
    public string? WarehouseCen { get; set; }
}

public class DefaultWarehouseContractResponse
{
    public string CompanyCen { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
}

public class SetDefaultWarehouseContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
}

public class CancelTicketContractRequest
{
    public string? Reason { get; set; }
}

public class CancelTicketContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AssignTicketWaiterContractRequest
{
    public string WaiterCen { get; set; } = string.Empty;
}

public class AssignTicketWaiterContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public string WaiterCen { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
}

// ── Ticket Items ──────────────────────────────────────────────────────────────

public class TicketItemContractResponse
{
    public string TicketItemCen { get; set; } = string.Empty;
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SentAt { get; set; }
    public int ResendCount { get; set; }
}

public class CreateTicketItemContractRequest
{
    public string ProductCen { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class UpdateTicketItemContractRequest
{
    public int? Quantity { get; set; }
    public string? Note { get; set; }
}

// ── Ticket Totals ─────────────────────────────────────────────────────────────

public class TicketTotalsContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}

// ── Ticket Payment ────────────────────────────────────────────────────────────

public class PayTicketContractRequest
{
    public string PaymentMethodCode { get; set; } = string.Empty;
}

public class PayTicketContractResponse
{
    public string SaleCen { get; set; } = string.Empty;
    public string TicketCen { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? InventoryDocumentCen { get; set; }
}

public class ProcessRestaurantOrderPaymentResultDto
{
    public bool IsSuccess { get; set; }
    public int? SaleId { get; set; }
    public string? SaleCen { get; set; }
    public string? InventoryDocumentCen { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<StockInsufficiencyResponseDto> Insufficiencies { get; set; } = new();
}

public class StockInsufficiencyResponseDto
{
    public int ProductId { get; set; }
    public string? ProductCen { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? WarehouseCen { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MissingQuantity { get; set; }
}

// ── Waiters ───────────────────────────────────────────────────────────────────

public class WaiterContractResponse
{
    public string WaiterCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
