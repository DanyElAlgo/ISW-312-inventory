using Sales.API.DTOs;
using Sales.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Services;

public class StationManagementService
{
    private readonly SalesDbContext _context;

    public StationManagementService(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<List<StationTypeDto>> GetAllStationTypesAsync()
    {
        var types = await _context.StationTypes
            .Include(t => t.Stations)
            .ToListAsync();

        var result = new List<StationTypeDto>();
        foreach (var t in types)
        {
            var categoryIds = await GetCoverageForTypeAsync(t.Id);
            result.Add(MapTypeToDto(t, categoryIds));
        }
        return result;
    }

    public async Task<StationTypeDto?> GetStationTypeByIdAsync(int id)
    {
        var t = await _context.StationTypes
            .Include(t => t.Stations)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (t == null)
            return null;

        var categoryIds = await GetCoverageForTypeAsync(id);
        return MapTypeToDto(t, categoryIds);
    }

    public async Task<StationTypeDto> CreateStationTypeAsync(StationTypeCreateDto dto)
    {
        var stationType = new StationType { Name = dto.Name, Description = dto.Description };
        _context.StationTypes.Add(stationType);
        await _context.SaveChangesAsync();
        return MapTypeToDto(stationType, new List<int>());
    }

    public async Task<StationTypeDto?> UpdateStationTypeAsync(int id, StationTypeUpdateDto dto)
    {
        var stationType = await _context.StationTypes
            .Include(t => t.Stations)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (stationType == null)
            return null;

        if (dto.Name != null)
            stationType.Name = dto.Name;
        if (dto.Description != null)
            stationType.Description = dto.Description;

        await _context.SaveChangesAsync();

        var categoryIds = await GetCoverageForTypeAsync(id);
        return MapTypeToDto(stationType, categoryIds);
    }

    public async Task<bool> DeleteStationTypeAsync(int id)
    {
        var stationType = await _context.StationTypes
            .Include(t => t.Stations)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (stationType == null)
            return false;

        if (stationType.Stations.Any())
            throw new InvalidOperationException("Cannot delete station type with existing stations.");

        // Delete coverage entries first
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM sales.station_coverage WHERE station_type_id = {0}", id);

        _context.StationTypes.Remove(stationType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<int>> GetCoverageForTypeAsync(int stationTypeId)
    {
        return await _context.Database
            .SqlQueryRaw<int>(
                "SELECT category_id AS \"Value\" FROM sales.station_coverage WHERE station_type_id = {0}",
                stationTypeId)
            .ToListAsync();
    }

    public async Task SetCoverageForTypeAsync(int stationTypeId, CoverageAssignDto dto)
    {
        var typeExists = await _context.StationTypes.AnyAsync(t => t.Id == stationTypeId);
        if (!typeExists)
            throw new InvalidOperationException("Station type not found.");

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM sales.station_coverage WHERE station_type_id = {0}", stationTypeId);

        foreach (var categoryId in dto.CategoryIds.Distinct())
        {
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO sales.station_coverage (station_type_id, category_id) VALUES ({0}, {1})",
                stationTypeId, categoryId);
        }
    }

    public async Task<List<StationDto>> GetAllStationsAsync()
    {
        var stations = await _context.Stations
            .Include(s => s.Type)
            .ToListAsync();

        return stations.Select(s => new StationDto
        {
            Id = s.Id,
            Name = s.Name ?? string.Empty,
            TypeId = s.TypeId ?? 0,
            TypeName = s.Type?.Name
        }).ToList();
    }

    public async Task<StationDto> CreateStationAsync(StationCreateDto dto)
    {
        var typeExists = await _context.StationTypes.AnyAsync(t => t.Id == dto.TypeId);
        if (!typeExists)
            throw new InvalidOperationException("Station type not found.");

        var station = new Station { Name = dto.Name, TypeId = dto.TypeId };
        _context.Stations.Add(station);
        await _context.SaveChangesAsync();

        return new StationDto
        {
            Id = station.Id,
            Name = station.Name,
            TypeId = station.TypeId ?? 0,
            TypeName = (await _context.StationTypes.FindAsync(dto.TypeId))?.Name
        };
    }

    public async Task<bool> DeleteStationAsync(int id)
    {
        var station = await _context.Stations.FindAsync(id);
        if (station == null)
            return false;

        _context.Stations.Remove(station);
        await _context.SaveChangesAsync();
        return true;
    }

    private static StationTypeDto MapTypeToDto(StationType t, List<int> categoryIds) => new()
    {
        Id = t.Id,
        Name = t.Name ?? string.Empty,
        Description = t.Description,
        CategoryIds = categoryIds,
        Stations = (t.Stations ?? new List<Station>()).Select(s => new StationDto
        {
            Id = s.Id,
            Name = s.Name ?? string.Empty,
            TypeId = s.TypeId ?? t.Id,
            TypeName = t.Name
        }).ToList()
    };
}
