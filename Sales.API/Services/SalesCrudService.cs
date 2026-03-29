using Sales.API.DTOs;
using Sales.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Services;

public class SalesCrudService
{
    private readonly SalesDbContext _context;

    public SalesCrudService(SalesDbContext context)
    {
        _context = context;
    }

    // ─── Customers ───────────────────────────────────────────────────────────

    public async Task<List<CustomerGetDto>> GetCustomersAsync()
    {
        return await _context.Customers
            .Select(c => new CustomerGetDto
            {
                Id = c.Id,
                Name = c.Name ?? string.Empty,
                Phone = c.Phone
            })
            .ToListAsync();
    }

    public async Task<CustomerGetDto?> GetCustomerByIdAsync(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null) return null;
        return new CustomerGetDto { Id = c.Id, Name = c.Name ?? string.Empty, Phone = c.Phone };
    }

    public async Task<CustomerGetDto> CreateCustomerAsync(CustomerCreateDto dto)
    {
        var customer = new Customer { Name = dto.Name, Phone = dto.Phone };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return new CustomerGetDto { Id = customer.Id, Name = customer.Name ?? string.Empty, Phone = customer.Phone };
    }

    public async Task<CustomerGetDto?> UpdateCustomerAsync(int id, CustomerUpdateDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return null;

        if (dto.Name != null) customer.Name = dto.Name;
        if (dto.Phone != null) customer.Phone = dto.Phone;

        await _context.SaveChangesAsync();
        return new CustomerGetDto { Id = customer.Id, Name = customer.Name ?? string.Empty, Phone = customer.Phone };
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return false;
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    // ─── Order Statuses (reference data) ─────────────────────────────────────

    public async Task<List<OrderStatusGetDto>> GetOrderStatusesAsync()
    {
        return await _context.OrderStatuses
            .Select(s => new OrderStatusGetDto
            {
                Id = s.Id,
                Name = s.Name ?? string.Empty,
                Description = s.Description
            })
            .ToListAsync();
    }

    // ─── Payment Types (reference data) ──────────────────────────────────────

    public async Task<List<PaymentTypeGetDto>> GetPaymentTypesAsync()
    {
        return await _context.PaymentTypes
            .Select(p => new PaymentTypeGetDto
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                Description = p.Description
            })
            .ToListAsync();
    }

    // ─── Orders (OrderTickets) ────────────────────────────────────────────────

    public async Task<List<OrderGetDto>> GetOrdersAsync()
    {
        return await _context.OrderTickets
            .Include(t => t.Customer)
            .Include(t => t.Status)
            .Select(t => new OrderGetDto
            {
                Id = t.Id,
                CustomerId = t.CustomerId,
                CustomerName = t.Customer != null ? t.Customer.Name : null,
                StatusId = t.StatusId,
                StatusName = t.Status != null ? t.Status.Name : null
            })
            .ToListAsync();
    }

    public async Task<OrderGetDto?> GetOrderByIdAsync(int id)
    {
        var t = await _context.OrderTickets
            .Include(x => x.Customer)
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (t == null) return null;

        return new OrderGetDto
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            CustomerName = t.Customer?.Name,
            StatusId = t.StatusId,
            StatusName = t.Status?.Name
        };
    }

    public async Task<OrderGetDto> CreateOrderAsync(OrderCreateDto dto)
    {
        var ticket = new OrderTicket
        {
            CustomerId = dto.CustomerId,
            StatusId = dto.StatusId
        };
        _context.OrderTickets.Add(ticket);
        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(ticket.Id) ?? throw new InvalidOperationException("Could not create order.");
    }

    public async Task<OrderGetDto?> UpdateOrderAsync(int id, OrderUpdateDto dto)
    {
        var ticket = await _context.OrderTickets.FindAsync(id);
        if (ticket == null) return null;

        if (dto.CustomerId.HasValue) ticket.CustomerId = dto.CustomerId.Value;
        if (dto.StatusId.HasValue) ticket.StatusId = dto.StatusId.Value;

        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(id);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var ticket = await _context.OrderTickets.FindAsync(id);
        if (ticket == null) return false;
        _context.OrderTickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    // ─── Order Items ──────────────────────────────────────────────────────────

    public async Task<List<OrderItemGetDto>> GetOrderItemsAsync(int orderId)
    {
        return await _context.OrderItems
            .Include(i => i.Status)
            .Where(i => i.OrderId == orderId)
            .Select(i => new OrderItemGetDto
            {
                Id = i.Id,
                Qty = i.Qty ?? 0,
                AdditionalNote = i.AdditionalNote,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                UnitPrice = i.UnitPrice ?? 0,
                ProductName = i.ProductName,
                StatusId = i.StatusId,
                StatusName = i.Status != null ? i.Status.Name : null
            })
            .ToListAsync();
    }

    public async Task<OrderItemGetDto> CreateOrderItemAsync(OrderItemCreateDto dto)
    {
        var item = new OrderItem
        {
            Qty = dto.Qty,
            AdditionalNote = dto.AdditionalNote,
            OrderId = dto.OrderId,
            ProductId = dto.ProductId,
            StatusId = dto.StatusId
        };
        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();

        var created = await _context.OrderItems
            .Include(i => i.Status)
            .FirstAsync(i => i.Id == item.Id);

        return new OrderItemGetDto
        {
            Id = created.Id,
            Qty = created.Qty ?? 0,
            AdditionalNote = created.AdditionalNote,
            OrderId = created.OrderId,
            ProductId = created.ProductId,
            UnitPrice = created.UnitPrice ?? 0,
            ProductName = created.ProductName,
            StatusId = created.StatusId,
            StatusName = created.Status?.Name
        };
    }

    public async Task<OrderItemGetDto?> UpdateOrderItemAsync(int id, OrderItemUpdateDto dto)
    {
        var item = await _context.OrderItems.Include(i => i.Status).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return null;

        if (dto.Qty.HasValue) item.Qty = dto.Qty.Value;
        if (dto.AdditionalNote != null) item.AdditionalNote = dto.AdditionalNote;
        if (dto.StatusId.HasValue) item.StatusId = dto.StatusId.Value;

        await _context.SaveChangesAsync();

        return new OrderItemGetDto
        {
            Id = item.Id,
            Qty = item.Qty ?? 0,
            AdditionalNote = item.AdditionalNote,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            UnitPrice = item.UnitPrice ?? 0,
            ProductName = item.ProductName,
            StatusId = item.StatusId,
            StatusName = item.Status?.Name
        };
    }

    public async Task<bool> DeleteOrderItemAsync(int id)
    {
        var item = await _context.OrderItems.FindAsync(id);
        if (item == null) return false;
        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    // ─── Payments ─────────────────────────────────────────────────────────────

    public async Task<List<PaymentGetDto>> GetPaymentsAsync(int orderId)
    {
        return await _context.Payments
            .Include(p => p.PaymentType)
            .Where(p => p.OrderId == orderId)
            .Select(p => new PaymentGetDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                PaymentTypeId = p.PaymentTypeId,
                PaymentTypeName = p.PaymentType != null ? p.PaymentType.Name : null,
                PaidAt = p.PaidAt.HasValue ? p.PaidAt.Value.ToString("o") : string.Empty
            })
            .ToListAsync();
    }

    public async Task<PaymentGetDto> CreatePaymentAsync(PaymentCreateDto dto)
    {
        var payment = new Payment
        {
            OrderId = dto.OrderId,
            PaymentTypeId = dto.PaymentTypeId,
            PaidAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var created = await _context.Payments
            .Include(p => p.PaymentType)
            .FirstAsync(p => p.Id == payment.Id);

        return new PaymentGetDto
        {
            Id = created.Id,
            OrderId = created.OrderId,
            PaymentTypeId = created.PaymentTypeId,
            PaymentTypeName = created.PaymentType?.Name,
            PaidAt = created.PaidAt.HasValue ? created.PaidAt.Value.ToString("o") : string.Empty
        };
    }
}
