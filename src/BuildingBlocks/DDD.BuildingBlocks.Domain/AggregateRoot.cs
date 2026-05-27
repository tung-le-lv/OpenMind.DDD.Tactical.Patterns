namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for Aggregate Roots in the domain.
/// An Aggregate is a cluster of domain objects that can be treated as a single unit (Eric Evans, DDD).
/// The Aggregate Root is the only member of the Aggregate that outside objects are allowed to hold references to.
/// 
/// Key DDD principles for Aggregates:
/// 1. Reference only Aggregate Roots from outside the Aggregate boundary
/// 2. Use eventual consistency across Aggregate boundaries
/// 3. Keep Aggregates small - design around consistency boundaries
/// 4. Protect invariants within Aggregate boundaries
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    /// <summary>
    /// Version number for optimistic concurrency control.
    /// Incremented each time the aggregate is modified.
    /// </summary>
    public int Version { get; protected set; }

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Raises a domain event indicating something has happened in the domain.
    /// Domain events are facts about what happened in the past.
    /// </summary>
    protected void Emit(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }

    /// <summary>
    /// Increments the version for optimistic concurrency.
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }
}

/// <summary>
/// Marker interface for Aggregate Roots.
/// Used by repositories to ensure only Aggregate Roots can be persisted directly.
/// </summary>
public interface IAggregateRoot
{
    int Version { get; }
}
