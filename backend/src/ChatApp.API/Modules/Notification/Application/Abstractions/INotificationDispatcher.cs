namespace ChatApp.API.Modules.Notification.Application.Abstractions;

public interface INotificationDispatcher
{
    Task<bool> DispatchAsync(
        Domain.Notification notification,
        CancellationToken ct = default);

    Task DispatchPendingAsync(
        Guid userId,
        string connectionId,
        CancellationToken ct = default);
}