using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using System.Collections.Concurrent;

namespace ChatApp.API.Modules.Shared.Kernel.Mediator;

public sealed class Sender : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerType = HandlerTypeCache.GetOrAdd(requestType, rt =>
            typeof(IRequestHandler<,>).MakeGenericType(rt, responseType));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;

        var behaviors = GetPipelineBehaviors(requestType, responseType);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            return (Task<TResponse>)handleMethod
                .Invoke(handler, new object[] { request, cancellationToken })!;
        };

        for (int i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var behaviorHandle = behavior.GetType().GetMethod("Handle")!;
            var next = handlerDelegate;
            handlerDelegate = () => (Task<TResponse>)behaviorHandle
                .Invoke(behavior, new object[] { request, next, cancellationToken })!;
        }

        return handlerDelegate();
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        var responseType = typeof(Unit);
        var handlerType = HandlerTypeCache.GetOrAdd(requestType, rt =>
            typeof(IRequestHandler<,>).MakeGenericType(rt, responseType));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<Unit>, Unit>.Handle))!;

        return (Task)handleMethod
            .Invoke(handler, new object[] { request, cancellationToken })!;
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private List<object> GetPipelineBehaviors(Type requestType, Type responseType)
    {
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        return _serviceProvider.GetServices(behaviorType).ToList();
    }
}