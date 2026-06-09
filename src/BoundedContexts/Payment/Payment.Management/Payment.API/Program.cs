using BuildingBlocks.Integration;
using MongoDB.Driver;
using Order.Application.Services;
using Order.Contracts;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Repositories;
using Order.Infrastructure.Repositories;
using OrderMongoConfig = Order.Infrastructure.Persistence.MongoDbConfiguration;
using OrderMongoDbContext = Order.Infrastructure.Persistence.OrderMongoDbContext;
using Payment.Application.Handlers;
using Payment.Application.IntegrationEventHandlers;
using Payment.Domain.Repositories;
using Payment.Domain.Services;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configure MongoDB serialization for DDD entities
MongoDbConfiguration.Configure();    // Payment conventions
OrderMongoConfig.Configure();        // Order conventions

// MongoDB configuration
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>() ?? new MongoDbSettings();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));
builder.Services.AddScoped<PaymentMongoDbContext>();

// MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreatePaymentCommandHandler).Assembly);
});

// Repository
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Customer-Supplier: register Order's data provider so Payment can query order data.
// Payment.Application depends only on IOrderDataProvider (the contract).
// The concrete OrderDataProvider and its OrderRepository are wired here at the composition root.
builder.Services.AddScoped<OrderMongoDbContext>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDataProvider, OrderDataProvider>();

// Domain Services
builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();

// Event Bus
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Integration Event Handlers
builder.Services.AddScoped<IIntegrationEventHandler<OrderSubmittedIntegrationEvent>, OrderSubmittedIntegrationEventHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Payment Service API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Subscribe to integration events
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderSubmittedIntegrationEvent, OrderSubmittedIntegrationEventHandler>();

app.Run();
