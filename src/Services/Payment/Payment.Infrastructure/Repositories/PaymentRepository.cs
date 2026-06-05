using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using MongoDB.Driver;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.Repositories;
using Payment.Domain.Specifications;
using Payment.Domain.ValueObjects;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class PaymentRepository(PaymentMongoDbContext context) : IPaymentRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Domain.Aggregates.PaymentAggregate.Payment?> GetByIdAsync(
        PaymentId id,
        CancellationToken cancellationToken = default)
    {
        var doc = await context.Payments
            .Find(p => p.Id == id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return doc is null ? null : PaymentMapper.ToDomain(doc);
    }

    public Task<Domain.Aggregates.PaymentAggregate.Payment> AddAsync(
        Domain.Aggregates.PaymentAggregate.Payment aggregate,
        CancellationToken cancellationToken = default)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        var doc = PaymentMapper.ToDocument(aggregate);
        context.AddCommand(() => context.Payments.InsertOneAsync(doc, cancellationToken: cancellationToken));

        return Task.FromResult(aggregate);
    }

    public void Update(Domain.Aggregates.PaymentAggregate.Payment aggregate)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();

        var doc = PaymentMapper.ToDocument(aggregate);
        context.AddCommand(() => context.Payments.ReplaceOneAsync(
            p => p.Id == doc.Id,
            doc));
    }

    public void Remove(Domain.Aggregates.PaymentAggregate.Payment aggregate)
    {
        var id = aggregate.Id.Value;
        context.AddCommand(() => context.Payments.DeleteOneAsync(p => p.Id == id));
    }

    public async Task<bool> ExistsAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Find(p => p.Id == id.Value)
            .AnyAsync(cancellationToken);
    }

    public async Task<Domain.Aggregates.PaymentAggregate.Payment?> GetByOrderIdAsync(
        OrderReference orderId,
        CancellationToken cancellationToken = default)
    {
        var doc = await context.Payments
            .Find(p => p.OrderId == orderId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return doc is null ? null : PaymentMapper.ToDomain(doc);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetByCustomerIdAsync(
        CustomerReference customerId,
        CancellationToken cancellationToken = default)
    {
        var docs = await context.Payments
            .Find(p => p.CustomerId == customerId.Value)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(PaymentMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        var docs = await context.Payments
            .Find(p => p.Status == status.Name)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(PaymentMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetPendingPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new PendingPaymentSpecification();
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> FindAsync(
        Specification<Domain.Aggregates.PaymentAggregate.Payment> specification,
        CancellationToken cancellationToken = default)
    {
        // Specifications express domain-level predicates that reference value objects
        // and behaviour unavailable at the document layer. Load and apply in memory.
        // For high-volume collections, push a pre-filter down to MongoDB as an optimisation.
        var docs = await context.Payments.Find(_ => true).ToListAsync(cancellationToken);
        var predicate = specification.ToExpression().Compile();
        return docs.Select(PaymentMapper.ToDomain).Where(predicate).ToList();
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetRefundablePaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new RefundablePaymentSpecification();
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetFailedPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new FailedPaymentSpecification();
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetHighValuePaymentsAsync(
        decimal threshold = 1000m,
        CancellationToken cancellationToken = default)
    {
        var specification = new HighValuePaymentSpecification(threshold);
        return await FindAsync(specification, cancellationToken);
    }
}
