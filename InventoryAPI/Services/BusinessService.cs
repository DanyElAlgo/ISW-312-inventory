using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class BusinessService
{
    private readonly BusinessRepository _repository;

    public BusinessService(BusinessRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BusinessGetDto>> GetAllBusinessesAsync()
    {
        var businesses = await _repository.GetAllAsync();
        return businesses.Select(MapToGetDto);
    }

    public async Task<BusinessGetDto?> GetBusinessByIdAsync(int id)
    {
        var business = await _repository.GetByIdAsync(id);
        return business == null ? null : MapToGetDto(business);
    }

    public async Task<BusinessGetDto> CreateBusinessAsync(BusinessCreateDto dto)
    {
        var business = new Business
        {
            Name = dto.Name
        };

        var createdBusiness = await _repository.CreateAsync(business);
        return MapToGetDto(createdBusiness);
    }

    public async Task<BusinessGetDto?> UpdateBusinessAsync(int id, BusinessUpdateDto dto)
    {
        var business = await _repository.GetByIdAsync(id);
        if (business == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            business.Name = dto.Name;

        var updatedBusiness = await _repository.UpdateAsync(business);
        return MapToGetDto(updatedBusiness);
    }

    public async Task<bool> DeleteBusinessAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static BusinessGetDto MapToGetDto(Business business)
    {
        return new BusinessGetDto
        {
            Id = business.Id,
            Name = business.Name,
            WarehouseCount = business.Warehouses?.Count
        };
    }
}
