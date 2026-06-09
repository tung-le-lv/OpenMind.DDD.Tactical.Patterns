using BuildingBlocks.Integration;
using MongoDB.Driver;
using Order.Contracts;
using Order.Contracts.IntegrationEvents;
using Payment.Application.Commands;
using Payment.Application.IntegrationEventHandlers;
using Payment.Application.Services;
using Payment.Contracts;
using Payment.Domain.Repositories;
using Payment.Domain.Services;
using Payment.Infrastructure;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configure MongoDB serialization for DDD entities
MongoDbConfiguration.Configure();

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

// Customer-Supplier (customer-defined contract): Payment.Contracts defines ICustomerInfoProvider;
// Order.Application provides CustomerInfoProvider. ICustomerRepository requires a concrete
// infrastructure implementation in Order.Infrastructure before this can be resolved at runtime.
builder.Services.AddScoped<ICustomerInfoProvider, CustomerInfoProvider>();

// Payment gateway
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();

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
