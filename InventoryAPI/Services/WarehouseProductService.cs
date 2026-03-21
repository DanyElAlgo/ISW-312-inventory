using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class WarehouseProductService
{
    private const int ActiveStatusId = 1;
    private const int OutOfStockStatusId = 4;
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

    public async Task<StockOperationResultDto> SetInitialStockAsync(StockSetDto dto)
    {
        if (dto.Quantity < 0)
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Initial stock cannot be negative."
            };
        }

        if (!await _repository.ProductExistsAsync(dto.ProductId))
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Product not found."
            };
        }

        if (!await _repository.WarehouseExistsAsync(dto.WarehouseId))
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Warehouse not found."
            };
        }

        var warehouseProduct = await _repository.GetByWarehouseAndProductAsync(dto.WarehouseId, dto.ProductId);
        if (warehouseProduct == null)
        {
            warehouseProduct = new WarehouseProduct
            {
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                StockLeft = dto.Quantity,
                LowStockQty = 0,
                StatusId = dto.Quantity == 0 ? OutOfStockStatusId : ActiveStatusId
            };

            warehouseProduct = await _repository.CreateAsync(warehouseProduct);
        }
        else
        {
            warehouseProduct.StockLeft = dto.Quantity;
            warehouseProduct.StatusId = dto.Quantity == 0 ? OutOfStockStatusId : ActiveStatusId;
            warehouseProduct = await _repository.UpdateAsync(warehouseProduct);
        }

        var kardex = await _repository.AddKardexEntryAsync(new Kardex
        {
            WarehouseId = dto.WarehouseId,
            ProductId = dto.ProductId,
            ActionType = "INITIAL",
            ActionQty = dto.Quantity,
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Initial stock registration" : dto.Reason,
            TimeStamp = DateTime.UtcNow
        });

        return new StockOperationResultDto
        {
            Success = true,
            Message = "Initial stock registered successfully.",
            NewStock = warehouseProduct.StockLeft,
            KardexId = kardex.Id
        };
    }

    public async Task<StockOperationResultDto> AdjustStockAsync(StockChangeDto dto)
    {
        if (!string.Equals(dto.ActionType, "ENTRY", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(dto.ActionType, "EXIT", StringComparison.OrdinalIgnoreCase))
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "ActionType must be ENTRY or EXIT."
            };
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Reason is required for stock adjustments."
            };
        }

        var warehouseProduct = await _repository.GetByWarehouseAndProductAsync(dto.WarehouseId, dto.ProductId);
        if (warehouseProduct == null)
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Stock record for product and warehouse not found."
            };
        }

        var currentStock = warehouseProduct.StockLeft ?? 0;
        var isExit = string.Equals(dto.ActionType, "EXIT", StringComparison.OrdinalIgnoreCase);
        var newStock = isExit ? currentStock - dto.Quantity : currentStock + dto.Quantity;

        if (newStock < 0)
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Cannot reduce stock below 0."
            };
        }

        warehouseProduct.StockLeft = newStock;
        warehouseProduct.StatusId = newStock == 0 ? OutOfStockStatusId : ActiveStatusId;
        warehouseProduct = await _repository.UpdateAsync(warehouseProduct);

        var kardex = await _repository.AddKardexEntryAsync(new Kardex
        {
            WarehouseId = dto.WarehouseId,
            ProductId = dto.ProductId,
            ActionType = isExit ? "EXIT" : "ENTRY",
            ActionQty = dto.Quantity,
            Reason = dto.Reason.Trim(),
            TimeStamp = DateTime.UtcNow
        });

        return new StockOperationResultDto
        {
            Success = true,
            Message = "Stock adjusted successfully.",
            NewStock = warehouseProduct.StockLeft,
            KardexId = kardex.Id
        };
    }

    public async Task<StockOperationResultDto> SetOutOfStockAsync(int warehouseId, int productId, bool outOfStock)
    {
        var warehouseProduct = await _repository.GetByWarehouseAndProductAsync(warehouseId, productId);
        if (warehouseProduct == null)
        {
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Stock record for product and warehouse not found."
            };
        }

        warehouseProduct.StatusId = outOfStock ? OutOfStockStatusId : ActiveStatusId;
        warehouseProduct = await _repository.UpdateAsync(warehouseProduct);

        return new StockOperationResultDto
        {
            Success = true,
            Message = outOfStock
                ? "Product marked as out of stock."
                : "Product is available again.",
            NewStock = warehouseProduct.StockLeft
        };
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
