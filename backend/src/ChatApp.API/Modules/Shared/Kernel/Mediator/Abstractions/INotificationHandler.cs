namespace ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;

public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}