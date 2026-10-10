namespace ChatApp.API.Modules.Shared.Kernel.Abstractions;

public interface IEntity { }

public interface IEntity<TId> : IEntity
{
    TId Id { get; }
}
