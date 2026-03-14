using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class WarehouseService
{
    private readonly WarehouseRepository _repository;

    public WarehouseService(WarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WarehouseGetDto>> GetAllWarehousesAsync()
    {
        var warehouses = await _repository.GetAllAsync();
        return warehouses.Select(MapToGetDto);
    }

    public async Task<WarehouseGetDto?> GetWarehouseByIdAsync(int id)
    {
        var warehouse = await _repository.GetByIdAsync(id);
        return warehouse == null ? null : MapToGetDto(warehouse);
    }

    public async Task<IEnumerable<WarehouseGetDto>> GetWarehousesByBusinessIdAsync(int businessId)
    {
        var warehouses = await _repository.GetByBusinessIdAsync(businessId);
        return warehouses.Select(MapToGetDto);
    }

    public async Task<WarehouseGetDto> CreateWarehouseAsync(WarehouseCreateDto dto)
    {
        var warehouse = new Warehouse
        {
            BusinessId = dto.BusinessId,
            Name = dto.Name
        };

        var createdWarehouse = await _repository.CreateAsync(warehouse);
        return MapToGetDto(createdWarehouse);
    }

    public async Task<WarehouseGetDto?> UpdateWarehouseAsync(int id, WarehouseUpdateDto dto)
    {
        var warehouse = await _repository.GetByIdAsync(id);
        if (warehouse == null)
            return null;

        if (dto.BusinessId.HasValue)
            warehouse.BusinessId = dto.BusinessId;
        if (!string.IsNullOrEmpty(dto.Name))
            warehouse.Name = dto.Name;

        var updatedWarehouse = await _repository.UpdateAsync(warehouse);
        return MapToGetDto(updatedWarehouse);
    }

    public async Task<bool> DeleteWarehouseAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static WarehouseGetDto MapToGetDto(Warehouse warehouse)
    {
        return new WarehouseGetDto
        {
            Id = warehouse.Id,
            BusinessId = warehouse.BusinessId,
            BusinessName = warehouse.Business?.Name,
            Name = warehouse.Name,
            ProductCount = warehouse.WarehouseProducts?.Count
        };
    }
}
