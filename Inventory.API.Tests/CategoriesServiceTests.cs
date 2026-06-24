using Inventory.API.DTOs.Contract;
using Inventory.API.Models;
using Inventory.API.Repositories.Ef;
using Inventory.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.API.Tests;

/// <summary>
/// CRUD unit tests for <see cref="CategoriesService"/> (Inventory module).
/// Uses the EF Core InMemory provider so no real PostgreSQL is required, and wires
/// the real EF repositories over the in-memory context (no mocking framework needed).
/// </summary>
public class CategoriesServiceTests
{
    private const string CompanyCen = "BUS-000001";

    private static InventoryDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            // A unique database name per test isolates them from each other.
            .UseInMemoryDatabase($"inventory-tests-{Guid.NewGuid()}")
            .Options;

        return new InventoryDbContext(options);
    }

    private static CategoriesService NewService(InventoryDbContext context) =>
        new(
            context,
            new BusinessRepository(context),
            new CategoryRepository(context),
            new UnitRepository(context),
            new WarehouseRepository(context),
            new ProductRepository(context),
            new WarehouseProductRepository(context));

    private static async Task SeedBusinessAsync(InventoryDbContext context)
    {
        context.Businesses.Add(new Business { Name = "Acme Corp", Cen = CompanyCen, IsActive = true });
        await context.SaveChangesAsync();
    }

    // ---- CREATE ----

    [Fact]
    public async Task CreateCategoryAsync_PersistsCategory_AndGeneratesCen()
    {
        await using var context = NewContext();
        await SeedBusinessAsync(context);
        var service = NewService(context);

        var dto = await service.CreateCategoryAsync(
            CompanyCen, new CreateCategoryRequest { Name = "Beverages" });

        Assert.NotNull(dto);
        Assert.Equal("Beverages", dto!.Name);
        Assert.True(dto.IsActive);

        var stored = await context.Categories.SingleAsync();
        Assert.Equal("Beverages", stored.Name);
        Assert.Equal("CAT-000001", stored.Cen);
        Assert.Equal(stored.Cen, dto.CategoryCen);
    }

    [Fact]
    public async Task CreateCategoryAsync_Throws_WhenNameIsBlank()
    {
        await using var context = NewContext();
        await SeedBusinessAsync(context);
        var service = NewService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCategoryAsync(CompanyCen, new CreateCategoryRequest { Name = "   " }));

        Assert.Equal(0, await context.Categories.CountAsync());
    }

    // ---- READ ----

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllCategoriesForBusiness()
    {
        await using var context = NewContext();
        await SeedBusinessAsync(context);
        var service = NewService(context);
        await service.CreateCategoryAsync(CompanyCen, new CreateCategoryRequest { Name = "Beverages" });
        await service.CreateCategoryAsync(CompanyCen, new CreateCategoryRequest { Name = "Snacks" });

        var categories = await service.GetCategoriesAsync(CompanyCen);

        Assert.NotNull(categories);
        Assert.Equal(2, categories!.Count);
        Assert.Contains(categories, c => c.Name == "Beverages");
        Assert.Contains(categories, c => c.Name == "Snacks");
    }

    // ---- UPDATE ----

    [Fact]
    public async Task UpdateCategoryAsync_ChangesNameAndStatus()
    {
        await using var context = NewContext();
        await SeedBusinessAsync(context);
        var service = NewService(context);
        var created = await service.CreateCategoryAsync(
            CompanyCen, new CreateCategoryRequest { Name = "Beverages" });

        var updated = await service.UpdateCategoryAsync(
            CompanyCen, created!.CategoryCen,
            new UpdateCategoryRequest { Name = "Drinks", IsActive = false });

        Assert.NotNull(updated);
        Assert.Equal("Drinks", updated!.Name);
        Assert.False(updated.IsActive);

        var stored = await context.Categories.SingleAsync();
        Assert.Equal("Drinks", stored.Name);
        Assert.False(stored.IsActive);
    }
}
