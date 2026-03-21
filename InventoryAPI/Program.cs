using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;
using InventoryAPI.Repositories;
using InventoryAPI.Services;

// TODO: Change internal namespace name later

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and services
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
builder.Services.AddScoped<BusinessRepository>();
builder.Services.AddScoped<BusinessService>();

builder.Services.AddScoped<WarehouseRepository>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<PosService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS handling
// TODO: Change later
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

// TODO: Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll");
// app.UseAuthorization();
app.MapControllers();

app.Run();
