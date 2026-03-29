using System.ComponentModel.DataAnnotations;

namespace Sales.API.DTOs;

public class CustomerGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CustomerCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }
}

public class CustomerUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }
}

public class OrderStatusGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class PaymentTypeGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class OrderGetDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
}

public class OrderCreateDto
{
    public int CustomerId { get; set; }
    public int? StatusId { get; set; }
}

public class OrderUpdateDto
{
    public int? CustomerId { get; set; }
    public int? StatusId { get; set; }
}

public class OrderItemGetDto
{
    public int Id { get; set; }
    public double Qty { get; set; }
    public string? AdditionalNote { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ProductName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
}

public class OrderItemCreateDto
{
    [Range(typeof(double), "0.01", "79228162514264337593543950335")]
    public double Qty { get; set; }

    [MaxLength(255)]
    public string? AdditionalNote { get; set; }

    [Range(1, int.MaxValue)]
    public int OrderId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    public int? StatusId { get; set; }
}

public class OrderItemUpdateDto
{
    [Range(typeof(double), "0.01", "79228162514264337593543950335")]
    public double? Qty { get; set; }

    [MaxLength(255)]
    public string? AdditionalNote { get; set; }

    public int? StatusId { get; set; }
}

public class PaymentGetDto
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int? PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }
    public string PaidAt { get; set; } = string.Empty;
}

public class PaymentCreateDto
{
    [Range(1, int.MaxValue)]
    public int OrderId { get; set; }

    [Range(1, int.MaxValue)]
    public int PaymentTypeId { get; set; }
}
