using BuildingBlocks.Domain.BusinessRules;

namespace BuildingBlocks.Domain;

/// <summary>
/// Base class for all entities in the domain.
/// An Entity is defined by its identity, not by its attributes (Eric Evans, DDD).
/// Two entities are equal if they have the same identity, regardless of their attribute values.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    private int? _requestedHashCode;

    public TId Id { get; protected set; } = default!;

    private List<IDomainEvent>? _domainEvents;
    private List<IDomainEvent> DomainEventsList => _domainEvents ??= new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => DomainEventsList.AsReadOnly();

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Adds a domain event to be dispatched when the entity is persisted.
    /// Domain events represent something that happened in the domain that domain experts care about.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        DomainEventsList.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from the collection.
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        DomainEventsList.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events. Called after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    /// <summary>
    /// Checks a business rule and throws a BusinessRuleValidationException if it is broken.
    /// Use this method to enforce domain invariants within entity methods.
    /// 
    /// Unlike Specifications (which are "testers" for filtering/querying),
    /// Business Rules are "guards" that enforce policies and provide clear error messages.
    /// </summary>
    /// <param name="rule">The business rule to check.</param>
    /// <exception cref="BusinessRuleValidationException">Thrown when the rule is broken.</exception>
    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

    /// <summary>
    /// Checks multiple business rules and throws on the first broken rule.
    /// </summary>
    /// <param name="rules">The business rules to check.</param>
    /// <exception cref="BusinessRuleValidationException">Thrown when any rule is broken.</exception>
    protected static void CheckRules(params IBusinessRule[] rules)
    {
        foreach (var rule in rules)
        {
            CheckRule(rule);
        }
    }

    /// <summary>
    /// Determines whether the entity is transient (not yet persisted).
    /// </summary>
    public bool IsTransient()
    {
        return Id.Equals(default(TId));
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (IsTransient() || other.IsTransient())
        {
            return false;
        }

        return Id.Equals(other.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            _requestedHashCode ??= Id.GetHashCode() ^ 31;
            return _requestedHashCode.Value;
        }
        return base.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
