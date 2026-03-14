using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class WarehouseProductService
{
    private readonly WarehouseProductRepository _repository;

    public WarehouseProductService(WarehouseProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WarehouseProductGetDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToGetDto);
    }

    public async Task<WarehouseProductGetDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item == null ? null : MapToGetDto(item);
    }

    public async Task<IEnumerable<WarehouseProductGetDto>> GetByWarehouseIdAsync(int warehouseId)
    {
        var items = await _repository.GetByWarehouseIdAsync(warehouseId);
        return items.Select(MapToGetDto);
    }

    public async Task<IEnumerable<WarehouseProductGetDto>> GetByProductIdAsync(int productId)
    {
        var items = await _repository.GetByProductIdAsync(productId);
        return items.Select(MapToGetDto);
    }

    public async Task<IEnumerable<WarehouseProductGetDto>> GetLowStockItemsAsync()
    {
        var items = await _repository.GetLowStockItemsAsync();
        return items.Select(MapToGetDto);
    }

    public async Task<WarehouseProductGetDto> CreateAsync(WarehouseProductCreateDto dto)
    {
        var item = new WarehouseProduct
        {
            WarehouseId = dto.WarehouseId,
            ProductId = dto.ProductId,
            StatusId = dto.StatusId,
            StockLeft = dto.StockLeft,
            LowStockQty = dto.LowStockQty
        };

        var createdItem = await _repository.CreateAsync(item);
        return MapToGetDto(createdItem);
    }

    public async Task<WarehouseProductGetDto?> UpdateAsync(int id, WarehouseProductUpdateDto dto)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
            return null;

        if (dto.StatusId.HasValue)
            item.StatusId = dto.StatusId;
        if (dto.StockLeft.HasValue)
            item.StockLeft = dto.StockLeft;
        if (dto.LowStockQty.HasValue)
            item.LowStockQty = dto.LowStockQty;

        var updatedItem = await _repository.UpdateAsync(item);
        return MapToGetDto(updatedItem);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static WarehouseProductGetDto MapToGetDto(WarehouseProduct item)
    {
        return new WarehouseProductGetDto
        {
            Id = item.Id,
            WarehouseId = item.WarehouseId,
            WarehouseName = item.Warehouse?.Name,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name,
            StatusId = item.StatusId,
            StatusName = item.Status?.Name,
            StockLeft = item.StockLeft,
            LowStockQty = item.LowStockQty
        };
    }
}
