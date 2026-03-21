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
        ValidateName(dto.Name);

        if (await _repository.ExistsByNameAsync(dto.Name))
            throw new InvalidOperationException("A unit with the same name already exists.");

        var unit = new Unit
        {
            Name = dto.Name.Trim(),
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
        {
            ValidateName(dto.Name);

            if (await _repository.ExistsByNameAsync(dto.Name, id))
                throw new InvalidOperationException("A unit with the same name already exists.");

            unit.Name = dto.Name.Trim();
        }
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

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Unit name is required.");
    }
}
