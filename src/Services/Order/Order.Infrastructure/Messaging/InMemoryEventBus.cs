using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using BuildingBlocks.Integration;

namespace Order.Infrastructure.Messaging;

public class InMemoryEventBus(
    ILogger<InMemoryEventBus> logger,
    IServiceProvider serviceProvider) : IEventBus
{
    private readonly ConcurrentDictionary<string, List<Type>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var eventType = @event.GetType().Name;

        logger.LogInformation(
            "Publishing integration event {EventType} with Id {EventId}",
            eventType,
            @event.Id);

        if (_handlers.TryGetValue(eventType, out var handlerTypes))
        {
            foreach (var handlerType in handlerTypes)
            {
                var handler = serviceProvider.GetService(handlerType);
                if (handler != null)
                {
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(@event.GetType());
                    var method = concreteType.GetMethod("HandleAsync");
                    
                    if (method != null)
                    {
                        await (Task)method.Invoke(handler, new object[] { @event, cancellationToken })!;
                    }
                }
            }
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        _handlers.AddOrUpdate(
            eventType,
            new List<Type> { handlerType },
            (_, existing) =>
            {
                if (!existing.Contains(handlerType))
                {
                    existing.Add(handlerType);
                }

                return existing;
            });

        logger.LogInformation(
            "Subscribed {HandlerType} to {EventType}",
            handlerType.Name,
            eventType);
    }
}
