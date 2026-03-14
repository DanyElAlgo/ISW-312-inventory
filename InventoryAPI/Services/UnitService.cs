using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class UnitService
{
    private readonly UnitRepository _repository;

    public UnitService(UnitRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UnitGetDto>> GetAllUnitsAsync()
    {
        var units = await _repository.GetAllAsync();
        return units.Select(MapToGetDto);
    }

    public async Task<UnitGetDto?> GetUnitByIdAsync(int id)
    {
        var unit = await _repository.GetByIdAsync(id);
        return unit == null ? null : MapToGetDto(unit);
    }

    public async Task<UnitGetDto> CreateUnitAsync(UnitCreateDto dto)
    {
        var unit = new Unit
        {
            Name = dto.Name,
            Description = dto.Description
        };

        var createdUnit = await _repository.CreateAsync(unit);
        return MapToGetDto(createdUnit);
    }

    public async Task<UnitGetDto?> UpdateUnitAsync(int id, UnitUpdateDto dto)
    {
        var unit = await _repository.GetByIdAsync(id);
        if (unit == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            unit.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            unit.Description = dto.Description;

        var updatedUnit = await _repository.UpdateAsync(unit);
        return MapToGetDto(updatedUnit);
    }

    public async Task<bool> DeleteUnitAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static UnitGetDto MapToGetDto(Unit unit)
    {
        return new UnitGetDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Description = unit.Description
        };
    }
}
