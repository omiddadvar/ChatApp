namespace ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;

public interface IRequest<out TResponse> { }

public interface IRequest : IRequest<Unit> { }

public readonly struct Unit
{
    public static readonly Unit Value = new();
}
