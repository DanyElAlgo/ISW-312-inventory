using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Sales.API.HttpClients;
using Sales.API.Models;
using Sales.API.Services;

CultureInfo culture = new("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Typed HTTP client for cross-service calls to Inventory.API
builder.Services.AddHttpClient<InventoryClient>(client =>
{
    var baseUrl = builder.Configuration["InventoryApi:BaseUrl"]
        ?? throw new InvalidOperationException("InventoryApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<PosService>();
builder.Services.AddScoped<SalesCrudService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<StationManagementService>();

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
