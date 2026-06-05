using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using MongoDB.Driver;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.Repositories;
using Order.Domain.Specifications;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Repositories;

public class OrderRepository(OrderMongoDbContext context) : IOrderRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Domain.Aggregates.OrderAggregate.Order?> GetByIdAsync(
        OrderId id,
        CancellationToken cancellationToken = default)
    {
        var doc = await context.Orders
            .Find(o => o.Id == id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return doc is null ? null : OrderMapper.ToDomain(doc);
    }

    public Task<Domain.Aggregates.OrderAggregate.Order> AddAsync(
        Domain.Aggregates.OrderAggregate.Order aggregate,
        CancellationToken cancellationToken = default)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        var doc = OrderMapper.ToDocument(aggregate);
        context.AddCommand(() => context.Orders.InsertOneAsync(doc, cancellationToken: cancellationToken));

        return Task.FromResult(aggregate);
    }

    public void Update(Domain.Aggregates.OrderAggregate.Order aggregate)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        var doc = OrderMapper.ToDocument(aggregate);
        context.AddCommand(() => context.Orders.ReplaceOneAsync(
            o => o.Id == doc.Id,
            doc));
    }

    public void Remove(Domain.Aggregates.OrderAggregate.Order aggregate)
    {
        var id = aggregate.Id.Value;
        context.AddCommand(() => context.Orders.DeleteOneAsync(o => o.Id == id));
    }

    public async Task<bool> ExistsAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Find(o => o.Id == id.Value)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        var docs = await context.Orders
            .Find(o => o.CustomerId == customerId.Value)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(OrderMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var docs = await context.Orders
            .Find(o => o.Status == status.Name)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(OrderMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var docs = await context.Orders
            .Find(o => o.Status == OrderStatus.Submitted.Name)
            .SortBy(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(OrderMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> FindAsync(
        Specification<Domain.Aggregates.OrderAggregate.Order> specification,
        CancellationToken cancellationToken = default)
    {
        // Specifications express domain-level predicates (e.g. CanBeCancelled, IsOverdue)
        // that reference value objects and behaviour unavailable at the document layer.
        // We load all documents and apply the spec in memory after mapping.
        // For high-volume collections, push a pre-filter down to MongoDB as an optimisation,
        // while keeping this in-memory check as the authoritative guard.
        var docs = await context.Orders.Find(_ => true).ToListAsync(cancellationToken);
        var predicate = specification.ToExpression().Compile();
        return docs.Select(OrderMapper.ToDomain).Where(predicate).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetOverdueOrdersAsync(
        int hoursThreshold = 24,
        CancellationToken cancellationToken = default)
    {
        var specification = new OverdueOrderSpecification(hoursThreshold);
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetCancellableOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new CancellableOrderSpecification();
        return await FindAsync(specification, cancellationToken);
    }
}
