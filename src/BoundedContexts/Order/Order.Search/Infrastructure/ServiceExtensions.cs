using BuildingBlocks.Integration;
using Microsoft.Extensions.DependencyInjection;
using Order.Contracts.IntegrationEvents;
using Order.Search.Infrastructure.Messaging;
using Order.Search.Infrastructure.Persistence;
using Order.Search.IntegrationEventHandlers;

namespace Order.Search.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddOrderSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoDbConfiguration.Configure();

        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<OrderSearchMongoDbContext>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly));

        services.AddSingleton<IEventBus, InMemoryEventBus>();

        services.AddScoped<IIntegrationEventHandler<OrderCreatedIntegrationEvent>, OrderCreatedProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderItemAddedIntegrationEvent>, OrderItemAddedProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderStatusChangedIntegrationEvent>, OrderStatusChangedProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderPromotionAppliedIntegrationEvent>, OrderPromotionAppliedProjectionHandler>();

        return services;
    }

    public static async Task InitializeOrderSearchAsync(this IServiceProvider services)
    {
        var context = services.GetRequiredService<OrderSearchMongoDbContext>();
        await context.EnsureIndexesAsync();

        var eventBus = services.GetRequiredService<IEventBus>();
        eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedProjectionHandler>();
        eventBus.Subscribe<OrderItemAddedIntegrationEvent, OrderItemAddedProjectionHandler>();
        eventBus.Subscribe<OrderStatusChangedIntegrationEvent, OrderStatusChangedProjectionHandler>();
        eventBus.Subscribe<OrderPromotionAppliedIntegrationEvent, OrderPromotionAppliedProjectionHandler>();
    }
}
