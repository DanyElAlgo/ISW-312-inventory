using System.ComponentModel.DataAnnotations;
using Purchases.Domain.Enums;

namespace Purchases.API.DTOs;

// =============================================================================
// Contract DTOs — exact shapes from documentation/rafael-compras-v1.json
// =============================================================================

public sealed class CreatePurchaseOrderDto
{
    [Required]
    public required string SupplierCen { get; init; }

    [Required]
    public required string WarehouseCen { get; init; }

    [Required]
    public required IReadOnlyList<CreatePurchaseOrderItemDto> Items { get; init; }
}

public sealed class CreatePurchaseOrderItemDto
{
    [Required]
    public required string ProductCen { get; init; }

    public int Quantity { get; init; }
}

public sealed class PurchaseOrderSummaryDto
{
    public required string OrderCen { get; init; }
    public PurchaseStatusEnum Status { get; init; }
}

public sealed class PurchaseOrderListDto
{
    public required string OrderCen { get; init; }
    public PurchaseStatusEnum Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public required string SupplierCen { get; init; }
    public int ItemCount { get; init; }
}

public sealed class PurchaseOrderDetailDto
{
    public required string OrderCen { get; init; }
    public PurchaseStatusEnum Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public required string SupplierCen { get; init; }
    public required string WarehouseCen { get; init; }
    public required IReadOnlyList<PurchaseOrderDetailItemDto> Items { get; init; }
}

public sealed class PurchaseOrderDetailItemDto
{
    public required string ProductCen { get; init; }
    public int Quantity { get; init; }
}

public sealed class PurchaseOrderConfirmationDto
{
    public required string OrderCen { get; init; }
    public PurchaseStatusEnum Status { get; init; }
    public DateTime ConfirmedAt { get; init; }
}

public sealed class SupplierDto
{
    public required string SupplierCen { get; init; }
    public required string Name { get; init; }
}
