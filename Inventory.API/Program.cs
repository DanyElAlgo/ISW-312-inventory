using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;
using Inventory.API.Services;
using Inventory.API.Repositories.Ef;
using Inventory.API.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseProductRepository, WarehouseProductRepository>();

// Register all domain-specific services
builder.Services.AddScoped<CompaniesService>();
builder.Services.AddScoped<CategoriesService>();
builder.Services.AddScoped<UnitsService>();
builder.Services.AddScoped<WarehousesService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<DocumentsService>();
builder.Services.AddScoped<KardexService>();
builder.Services.AddScoped<StockValidationService>();
builder.Services.AddScoped<StockConsumeService>();

// Register facade service
builder.Services.AddScoped<InventoryContractService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

var enableSwagger = builder.Configuration.GetValue<bool>("Swagger:Enabled", app.Environment.IsDevelopment());
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
