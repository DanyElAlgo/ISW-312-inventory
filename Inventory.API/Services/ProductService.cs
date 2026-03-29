using Inventory.API.DTOs;
using Inventory.API.Models;
using Inventory.API.Repositories;
using Shared.Contracts.DTOs;

namespace Inventory.API.Services;

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
        await ValidateCreateAsync(dto);

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description,
            UnitId = dto.UnitId,
            UnitQty = dto.UnitQty,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            IsActive = true
        };

        var createdProduct = await _repository.CreateAsync(product);
        // Reload the product with related entities to populate Unit and Category names
        var refreshedProduct = await _repository.GetByIdAsync(createdProduct.Id);
        return MapToGetDto(refreshedProduct!);
    }

    public async Task<ProductGetDto?> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            product.Name = dto.Name.Trim();
        if (!string.IsNullOrEmpty(dto.Description))
            product.Description = dto.Description;
        if (dto.UnitId.HasValue)
        {
            if (!await _repository.UnitExistsAsync(dto.UnitId.Value))
                throw new InvalidOperationException("Unit does not exist.");

            product.UnitId = dto.UnitId;
        }
        if (dto.UnitQty.HasValue)
            product.UnitQty = dto.UnitQty;
        if (dto.CategoryId.HasValue)
        {
            if (!await _repository.CategoryExistsAsync(dto.CategoryId.Value))
                throw new InvalidOperationException("Category does not exist.");

            product.CategoryId = dto.CategoryId;
        }
        if (dto.Price.HasValue)
        {
            if (dto.Price.Value <= 0)
                throw new ArgumentException("Price must be greater than 0.");

            product.Price = dto.Price;
        }
        if (dto.IsActive.HasValue)
            product.IsActive = dto.IsActive.Value;

        ValidateProductState(product);

        var updatedProduct = await _repository.UpdateAsync(product);
        return MapToGetDto(updatedProduct);
    }

    public async Task<ProductGetDto?> SetProductActiveStatusAsync(int id, bool isActive)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;

        product.IsActive = isActive;
        var updatedProduct = await _repository.UpdateAsync(product);
        return MapToGetDto(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<ProductReferenceDto?> GetProductReferenceAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;

        var hasStock = await _repository.HasAvailableStockAsync(id);

        return new ProductReferenceDto
        {
            Id = product.Id,
            Name = product.Name ?? string.Empty,
            Price = product.Price ?? 0,
            IsActive = product.IsActive ?? false,
            CategoryId = product.CategoryId,
            HasAvailableStock = hasStock
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

    private static ProductGetDto MapToGetDto(Product product)
    {
        return new ProductGetDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsActive = product.IsActive ?? true,
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

    private async Task ValidateCreateAsync(ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Product name is required.");

        if (dto.Price <= 0)
            throw new ArgumentException("Price must be greater than 0.");

        if (!await _repository.UnitExistsAsync(dto.UnitId))
            throw new InvalidOperationException("Unit does not exist.");

        if (!await _repository.CategoryExistsAsync(dto.CategoryId))
            throw new InvalidOperationException("Category does not exist.");
    }

    private static void ValidateProductState(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required.");

        if (!product.CategoryId.HasValue)
            throw new ArgumentException("Category is required.");

        if (!product.UnitId.HasValue)
            throw new ArgumentException("Unit is required.");

        if (!product.Price.HasValue || product.Price.Value <= 0)
            throw new ArgumentException("Price must be greater than 0.");
    }
}

