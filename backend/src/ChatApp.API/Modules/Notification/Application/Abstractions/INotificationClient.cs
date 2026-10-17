using ChatApp.API.Modules.Notification.Application.DTOs;

namespace ChatApp.API.Modules.Notification.Application.Abstractions;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationEnvelope notification);
    Task UnreadCountChanged(int count);
}