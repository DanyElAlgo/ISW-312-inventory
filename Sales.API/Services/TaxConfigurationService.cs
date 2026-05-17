using Sales.API.DTOs;
using Sales.API.Repositories.Interfaces;

namespace Sales.API.Services;

public class TaxConfigurationService
{
    private readonly IGlobalTaxConfigRepository _config;
    private readonly ISalesUnitOfWork _uow;

    public TaxConfigurationService(IGlobalTaxConfigRepository config, ISalesUnitOfWork uow)
    {
        _config = config;
        _uow = uow;
    }

    public async Task<TaxConfigurationContractResponse> GetAsync(string companyCen)
    {
        var config = await _config.GetOrCreateAsync();
        return new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config.TaxRate
        };
    }

    public async Task<TaxConfigurationContractResponse> UpdateAsync(string companyCen, UpdateTaxConfigurationContractRequest request)
    {
        if (request.GlobalTaxPercentage < 0)
            throw new ArgumentException("Tax rate cannot be negative.");

        var config = await _config.GetOrCreateAsync();
        config.TaxRate = request.GlobalTaxPercentage;
        await _uow.SaveChangesAsync();

        return new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config.TaxRate
        };
    }
}
