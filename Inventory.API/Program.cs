using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;
using Inventory.API.Repositories;
using Inventory.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductSearchRepository>();
builder.Services.AddScoped<ProductSearchService>();

builder.Services.AddScoped<UnitRepository>();
builder.Services.AddScoped<UnitService>();

builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

builder.Services.AddScoped<WarehouseProductRepository>();
builder.Services.AddScoped<WarehouseProductService>();

builder.Services.AddScoped<BusinessRepository>();
builder.Services.AddScoped<BusinessService>();

builder.Services.AddScoped<WarehouseRepository>();
builder.Services.AddScoped<WarehouseService>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
