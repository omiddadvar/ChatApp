namespace ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;

public interface IPublisher
{
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
