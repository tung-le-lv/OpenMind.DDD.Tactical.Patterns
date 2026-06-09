using BuildingBlocks.Integration;
using MongoDB.Driver;
using Order.Application.AntiCorruption;
using Order.Application.Commands;
using Order.Application.IntegrationEventHandlers;
using Order.Domain.Repositories;
using Order.Domain.Services;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Repositories;
using Payment.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB serialization for DDD entities
MongoDbConfiguration.Configure();

var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>() ?? new MongoDbSettings();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));
builder.Services.AddScoped<OrderMongoDbContext>();

// MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly);
});

// Repository
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Domain Services
builder.Services.AddScoped<IOrderConsolidationService, OrderConsolidationService>();

// Anti-Corruption Layer
builder.Services.AddScoped<ExternalOrderTranslator>();

// Event Bus
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Integration Event Handlers
builder.Services.AddScoped<IIntegrationEventHandler<PaymentCompletedIntegrationEvent>, PaymentCompletedIntegrationEventHandler>();
builder.Services.AddScoped<IIntegrationEventHandler<PaymentFailedIntegrationEvent>, PaymentFailedIntegrationEventHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Service API", Version = "v1" });
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
eventBus.Subscribe<PaymentCompletedIntegrationEvent, PaymentCompletedIntegrationEventHandler>();
eventBus.Subscribe<PaymentFailedIntegrationEvent, PaymentFailedIntegrationEventHandler>();

app.Run();
