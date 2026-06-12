using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Purchases.API;
using Purchases.API.Http;
using Purchases.API.HttpClients;
using Purchases.API.Models;
using Purchases.API.Repositories.Ef;
using Purchases.API.Repositories.Interfaces;
using Purchases.API.Services;

DotNetEnv.Env.TraversePath().Load();

CultureInfo culture = new("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// Inventory base URL — shared .env name first, with fallbacks so nothing breaks.
var inventoryUrl = builder.Configuration["INVENTORY_URL"]
    ?? builder.Configuration["Modules:InventoryBaseUrl"]
    ?? builder.Configuration["InventoryApi:BaseUrl"]
    ?? throw new InvalidOperationException("INVENTORY_URL not configured");

builder.Services.AddDbContext<PurchasesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<InventoryFailureTranslatingHandler>();
builder.Services.AddHttpClient<InventoryClient>(client =>
    {
        client.BaseAddress = new Uri(inventoryUrl);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<InventoryFailureTranslatingHandler>()
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

// RFC-7807 ProblemDetails error schema (shared contract, Section 1.1).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration["CORS_ORIGINS"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        if (corsOrigins.Length > 0)
            policy.WithOrigins(corsOrigins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

var enableSwagger = builder.Configuration.GetValue<bool>("Swagger:Enabled", app.Environment.IsDevelopment());
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.MapControllers();

app.Run();
