using ChatApp.API.Modules.Notification.Domain.Enums;

namespace ChatApp.API.Modules.Notification.Application.Abstractions;

public interface INotificationService
{
    Task<Guid> CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? payloadJson = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Domain.Notification>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}