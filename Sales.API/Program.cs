using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Sales.API;
using Sales.API.Http;
using Sales.API.HttpClients;
using Sales.API.Models;
using Sales.API.Repositories.Ef;
using Sales.API.Repositories.Interfaces;
using Sales.API.Services;

CultureInfo culture = new("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<InventoryFailureTranslatingHandler>();
builder.Services.AddHttpClient<InventoryClient>(client =>
    {
        var baseUrl = builder.Configuration["InventoryApi:BaseUrl"]
            ?? throw new InvalidOperationException("InventoryApi:BaseUrl is not configured.");
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<InventoryFailureTranslatingHandler>()
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.Configure<InventoryIntegrationOptions>(
    builder.Configuration.GetSection("InventoryIntegration"));

// Repositories
builder.Services.AddScoped<IOrderTicketRepository, OrderTicketRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<IStationTypeRepository, StationTypeRepository>();
builder.Services.AddScoped<IWaiterRepository, WaiterRepository>();
builder.Services.AddScoped<IGlobalTaxConfigRepository, GlobalTaxConfigRepository>();
builder.Services.AddScoped<IOrderCommandRepository, OrderCommandRepository>();
builder.Services.AddScoped<ICommandItemRepository, CommandItemRepository>();
builder.Services.AddScoped<IDefaultWarehouseRepository, DefaultWarehouseRepository>();
builder.Services.AddScoped<ISalesUnitOfWork, SalesUnitOfWork>();

// Per-entity services
builder.Services.AddScoped<OrderStatusesService>();
builder.Services.AddScoped<WaitersService>();
builder.Services.AddScoped<PaymentMethodsService>();
builder.Services.AddScoped<TaxConfigurationService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<DefaultWarehouseService>();
builder.Services.AddScoped<OrderTicketsService>();
builder.Services.AddScoped<OrderItemsService>();
builder.Services.AddScoped<KdsService>();
builder.Services.AddScoped<PaymentsService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();

var enableSwagger = builder.Configuration.GetValue<bool>("Swagger:Enabled", app.Environment.IsDevelopment());
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
