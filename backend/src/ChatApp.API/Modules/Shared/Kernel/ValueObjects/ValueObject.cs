namespace ChatApp.API.Modules.Shared.Kernel.ValueObjects;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) => obj is ValueObject other && ValuesAreEqual(other);
    public bool Equals(ValueObject? other) => other is not null && ValuesAreEqual(other);

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(17, (h, c) => HashCode.Combine(h, c?.GetHashCode() ?? 0));

    private bool ValuesAreEqual(ValueObject other) =>
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public static bool operator ==(ValueObject? l, ValueObject? r) => Equals(l, r);
    public static bool operator !=(ValueObject? l, ValueObject? r) => !Equals(l, r);
}