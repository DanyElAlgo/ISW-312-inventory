using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Repositories;

namespace InventoryAPI.Services;

public class ProductService
{
    private readonly ProductRepository _repository;

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
            Description = dto.Description,
            UnitId = dto.UnitId,
            UnitQty = dto.UnitQty,
            CategoryId = dto.CategoryId
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
        if (!string.IsNullOrEmpty(dto.Description))
            product.Description = dto.Description;
        if (dto.UnitId.HasValue)
            product.UnitId = dto.UnitId;
        if (dto.UnitQty.HasValue)
            product.UnitQty = dto.UnitQty;
        if (dto.CategoryId.HasValue)
            product.CategoryId = dto.CategoryId;

        var updatedProduct = await _repository.UpdateAsync(product);
        return MapToGetDto(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _repository.DeleteAsync(id);
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

    private static ProductGetDto MapToGetDto(Product product)
    {
        return new ProductGetDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            UnitId = product.UnitId,
            UnitName = product.Unit?.Name,
            UnitQty = product.UnitQty,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name
        };
    }

    private static KardexGetDto MapKardexToDto(Kardex kardex)
    {
        return new KardexGetDto
        {
            Id = kardex.Id,
            WarehouseId = kardex.WarehouseId,
            ProductId = kardex.ProductId,
            ActionType = kardex.ActionType,
            ActionQty = kardex.ActionQty,
            Reason = kardex.Reason,
            TimeStamp = kardex.TimeStamp
        };
    }
}

