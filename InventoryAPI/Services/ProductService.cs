using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class ProductService
{
    private readonly ProductRepository _repository;
    private const int DefaultBusinessId = 1; // debug

    public ProductService(ProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductGetDto>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(MapToGetDto);
    }

    public async Task<ProductGetDto?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : MapToGetDto(product);
    }

    public async Task<ProductGetDto> CreateProductAsync(ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Sku = dto.Sku,
            Category = dto.Category,
            Description = dto.Description,
            Metricunit = dto.MetricUnit,
            Status = dto.Status,
            Batch = dto.Batch,
            Stockleft = dto.InitialStock,
            Lowstockqty = dto.LowStockQty
        };

        var createdProduct = await _repository.CreateAsync(product);
        return MapToGetDto(createdProduct);
    }

    public async Task<ProductGetDto?> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Sku))
            product.Sku = dto.Sku;
        if (!string.IsNullOrEmpty(dto.Category))
            product.Category = dto.Category;
        if (!string.IsNullOrEmpty(dto.Description))
            product.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.MetricUnit))
            product.Metricunit = dto.MetricUnit;
        if (dto.Status.HasValue)
            product.Status = dto.Status.Value;
        if (dto.Batch.HasValue)
            product.Batch = dto.Batch.Value;
        if (dto.LowStockQty.HasValue)
            product.Lowstockqty = dto.LowStockQty.Value;

        var updatedProduct = await _repository.UpdateAsync(product);
        return MapToGetDto(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<StockOperationResultDto> SetStockAsync(int productId, int warehouseId, int newQuantity, string? reason)
    {
        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Product not found"
            };

        int oldQuantity = product.Stockleft;
        int difference = newQuantity - oldQuantity;
        string actionType = difference > 0 ? "ADD" : (difference < 0 ? "SUB" : "SET");

        product.Stockleft = newQuantity;
        await _repository.UpdateAsync(product);

        var kardexEntry = new Kardex
        {
            Warehouseprimaryid = warehouseId,
            Businessid = DefaultBusinessId,
            Productid = productId,
            Actiontype = actionType,
            Actionqty = Math.Abs(difference),
            Reason = reason ?? "Stock adjustment",
            Timestamp = DateTime.UtcNow
        };

        var kardex = await _repository.AddKardexEntryAsync(kardexEntry);

        return new StockOperationResultDto
        {
            Success = true,
            Message = $"Stock set to {newQuantity} units",
            NewStock = newQuantity,
            KardexId = kardex.Kardexid
        };
    }

    public async Task<StockOperationResultDto> AddStockAsync(int productId, int warehouseId, int quantity, string? reason)
    {
        if (quantity <= 0)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Quantity must be greater than 0"
            };

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Product not found"
            };

        product.Stockleft += quantity;
        await _repository.UpdateAsync(product);

        var kardexEntry = new Kardex
        {
            Warehouseprimaryid = warehouseId,
            Businessid = DefaultBusinessId,
            Productid = productId,
            Actiontype = "ADD",
            Actionqty = quantity,
            Reason = reason ?? "Stock addition",
            Timestamp = DateTime.UtcNow
        };

        var kardex = await _repository.AddKardexEntryAsync(kardexEntry);

        return new StockOperationResultDto
        {
            Success = true,
            Message = $"Added {quantity} units to stock",
            NewStock = product.Stockleft,
            KardexId = kardex.Kardexid
        };
    }

    public async Task<StockOperationResultDto> SubtractStockAsync(int productId, int warehouseId, int quantity, string? reason)
    {
        if (quantity <= 0)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Quantity must be greater than 0"
            };

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Product not found"
            };

        if (product.Stockleft < quantity)
            return new StockOperationResultDto
            {
                Success = false,
                Message = $"Insufficient stock. Current stock: {product.Stockleft}, requested: {quantity}"
            };

        product.Stockleft -= quantity;
        await _repository.UpdateAsync(product);

        var kardexEntry = new Kardex
        {
            Warehouseprimaryid = warehouseId,
            Businessid = DefaultBusinessId,
            Productid = productId,
            Actiontype = "SUB",
            Actionqty = quantity,
            Reason = reason ?? "Stock removal",
            Timestamp = DateTime.UtcNow
        };

        var kardex = await _repository.AddKardexEntryAsync(kardexEntry);

        return new StockOperationResultDto
        {
            Success = true,
            Message = $"Subtracted {quantity} units from stock",
            NewStock = product.Stockleft,
            KardexId = kardex.Kardexid
        };
    }

    public async Task<StockOperationResultDto> TransferStockAsync(int productId, int sourceWarehouseId, int destinationWarehouseId, int quantity, string? reason)
    {
        if (quantity <= 0)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Quantity must be greater than 0"
            };

        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
            return new StockOperationResultDto
            {
                Success = false,
                Message = "Product not found"
            };

        if (product.Stockleft < quantity)
            return new StockOperationResultDto
            {
                Success = false,
                Message = $"Insufficient stock to transfer. Current stock: {product.Stockleft}, requested: {quantity}"
            };

        product.Stockleft -= quantity;
        await _repository.UpdateAsync(product);

        var kardexEntry = new Kardex
        {
            Warehouseprimaryid = sourceWarehouseId,
            Warehousesecondaryid = destinationWarehouseId,
            Businessid = DefaultBusinessId,
            Productid = productId,
            Actiontype = "TRANSFER",
            Actionqty = quantity,
            Reason = reason ?? $"Transfer to warehouse {destinationWarehouseId}",
            Timestamp = DateTime.UtcNow
        };

        var kardex = await _repository.AddKardexEntryAsync(kardexEntry);

        return new StockOperationResultDto
        {
            Success = true,
            Message = $"Transferred {quantity} units from warehouse {sourceWarehouseId} to {destinationWarehouseId}",
            NewStock = product.Stockleft,
            KardexId = kardex.Kardexid
        };
    }

    public async Task<IEnumerable<KardexGetDto>> GetProductHistoryAsync(int productId)
    {
        var kardexEntries = await _repository.GetKardexByProductIdAsync(productId);
        return kardexEntries.Select(MapKardexToDto);
    }

    public async Task<IEnumerable<KardexGetDto>> GetWarehouseHistoryAsync(int warehouseId)
    {
        var kardexEntries = await _repository.GetKardexByWarehouseAsync(warehouseId);
        return kardexEntries.Select(MapKardexToDto);
    }

    public async Task<IEnumerable<KardexGetDto>> GetProductWarehouseHistoryAsync(int productId, int warehouseId)
    {
        var kardexEntries = await _repository.GetKardexByProductAndWarehouseAsync(productId, warehouseId);
        return kardexEntries.Select(MapKardexToDto);
    }

    private ProductGetDto MapToGetDto(Product product)
    {
        return new ProductGetDto
        {
            ProductId = product.Productid,
            Name = product.Name,
            Sku = product.Sku,
            Category = product.Category,
            Description = product.Description,
            MetricUnit = product.Metricunit,
            Status = product.Status,
            Batch = product.Batch,
            StockLeft = product.Stockleft,
            LowStockQty = product.Lowstockqty
        };
    }

    private KardexGetDto MapKardexToDto(Kardex kardex)
    {
        return new KardexGetDto
        {
            KardexId = kardex.Kardexid,
            WarehousePrimaryId = kardex.Warehouseprimaryid,
            WarehouseSecondaryId = kardex.Warehousesecondaryid,
            BusinessId = kardex.Businessid,
            ProductId = kardex.Productid,
            ActionType = kardex.Actiontype,
            ActionQty = kardex.Actionqty,
            Reason = kardex.Reason,
            Timestamp = kardex.Timestamp
        };
    }
}
