using Sales.API.DTOs;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class PaymentMethodsService
{
    private readonly IPaymentTypeRepository _paymentTypes;

    public PaymentMethodsService(IPaymentTypeRepository paymentTypes)
    {
        _paymentTypes = paymentTypes;
    }

    public async Task<IReadOnlyList<PaymentMethodContractResponse>> GetPaymentMethodsAsync()
    {
        var list = await _paymentTypes.GetAllAsync();
        return list.Select(pt => new PaymentMethodContractResponse
        {
            PaymentMethodCode = pt.Code ?? pt.Id.ToString(),
            Name = pt.Name ?? string.Empty,
            IsActive = pt.IsActive
        }).ToList();
    }
}
