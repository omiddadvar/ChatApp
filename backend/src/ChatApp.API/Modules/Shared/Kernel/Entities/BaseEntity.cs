using ChatApp.API.Modules.Shared.Kernel.Abstractions;

namespace ChatApp.API.Modules.Shared.Kernel.Entities;

public abstract class BaseEntity<TId> : IEntity<TId>, IEquatable<BaseEntity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj) => obj is BaseEntity<TId> other && Equals(other);
    public bool Equals(BaseEntity<TId>? other) =>
        other is not null && GetType() == other.GetType() && Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) => Equals(left, right);
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) => !Equals(left, right);
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() => Id = Guid.NewGuid();
}