using System.ComponentModel.DataAnnotations;

namespace Sales.API.DTOs;

public class GlobalTaxConfigDto
{
    public decimal TaxRate { get; set; }
}

public class OpenAccountCreateDto
{
    public int? CustomerId { get; set; }
}

public class AssignWaiterDto
{
    [Range(1, int.MaxValue)]
    public int WaiterId { get; set; }
}

public class AddOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(typeof(double), "0.01", "79228162514264337593543950335")]
    public double Quantity { get; set; }

    public string? Note { get; set; }
}

public class UpdateOrderItemDto
{
    [Range(typeof(double), "0.01", "79228162514264337593543950335")]
    public double? Quantity { get; set; }

    public string? Note { get; set; }
}

public class PosOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public double Quantity { get; set; }
    public string? Note { get; set; }
    public decimal LineTotal { get; set; }
}

public class PosAccountDto
{
    public int TicketId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? WaiterId { get; set; }
    public string? WaiterName { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<PosOrderItemDto> Items { get; set; } = new();
}

public class CommandSendResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CommandId { get; set; }
    public int ItemsSent { get; set; }
}

public class KdsItemDto
{
    public int CommandId { get; set; }
    public int TicketId { get; set; }
    public int OrderItemId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string StationType { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CheckoutDto
{
    [Range(1, int.MaxValue)]
    public int PaymentTypeId { get; set; }
}

public class CheckoutResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? PaymentId { get; set; }
    public decimal Total { get; set; }
}

public class CommandReprintItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Note { get; set; }
    public string StationName { get; set; } = string.Empty;
}

public class CommandReprintDto
{
    public int CommandId { get; set; }
    public int TicketId { get; set; }
    public string WaiterName { get; set; } = string.Empty;
    public DateTime PrintedAt { get; set; }
    public List<CommandReprintItemDto> Items { get; set; } = new();
}
