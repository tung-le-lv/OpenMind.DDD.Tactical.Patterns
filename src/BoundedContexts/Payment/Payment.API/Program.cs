using BuildingBlocks.Integration;
using MongoDB.Driver;
using Order.IntegrationEvents;
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
