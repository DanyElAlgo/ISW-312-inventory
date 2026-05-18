using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Purchases.API.HttpClients;
using Purchases.API.Models;
using Purchases.API.Repositories.Ef;
using Purchases.API.Repositories.Interfaces;
using Purchases.API.Services;

CultureInfo culture = new("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PurchasesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<InventoryClient>(client =>
{
    var baseUrl = builder.Configuration["InventoryApi:BaseUrl"]
        ?? throw new InvalidOperationException("InventoryApi:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.Configure<InventoryIntegrationOptions>(
    builder.Configuration.GetSection("InventoryIntegration"));

// Repositories
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IPurchaseStatusRepository, PurchaseStatusRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();
builder.Services.AddScoped<IPurchasesUnitOfWork, PurchasesUnitOfWork>();

// Per-entity services
builder.Services.AddScoped<PurchaseStatusesService>();
builder.Services.AddScoped<SuppliersService>();
builder.Services.AddScoped<PurchaseOrdersService>();

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
