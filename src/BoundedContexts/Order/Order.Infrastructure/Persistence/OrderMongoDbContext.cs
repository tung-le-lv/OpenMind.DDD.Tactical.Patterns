using BuildingBlocks.Domain;
using MediatR;
using MongoDB.Driver;
using Order.Infrastructure.Persistence.Documents;

namespace Order.Infrastructure.Persistence;

public class OrderMongoDbContext(IMongoDatabase database, IMediator mediator) : IUnitOfWork
{
    private readonly List<Func<Task>> _commands = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    public IMongoCollection<OrderDocument> Orders =>
        database.GetCollection<OrderDocument>("orders");

    public void AddCommand(Func<Task> command)
    {
        _commands.Add(command);
    }

    public void AddDomainEvents(IEnumerable<IDomainEvent> events)
    {
        _domainEvents.AddRange(events);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var count = _commands.Count;
        foreach (var command in _commands)
        {
            await command();
        }
        _commands.Clear();
        return count;
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events
        foreach (var domainEvent in _domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
        _domainEvents.Clear();

        // Execute all pending commands
        await SaveChangesAsync(cancellationToken);

        return true;
    }

    public void Dispose()
    {
        _commands.Clear();
        _domainEvents.Clear();
    }
}

