using System.Collections.Concurrent;
using BuildingBlocks.Integration;

namespace Order.Search.Infrastructure.Messaging;

public class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly ConcurrentDictionary<string, List<Type>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var eventType = @event.GetType().Name;

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
                        await (Task)method.Invoke(handler, [@event, cancellationToken])!;
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
                    existing.Add(handlerType);
                return existing;
            });
    }
}
