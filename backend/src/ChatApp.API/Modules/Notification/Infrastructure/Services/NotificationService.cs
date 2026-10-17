using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Notification.Domain.Enums;
using ChatApp.API.Modules.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Notification.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationDbContext _db;

    public NotificationService(NotificationDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(
        Guid userId, NotificationType type, string title, string body,
        string? payloadJson = null, CancellationToken ct = default)
    {
        var notification = Domain.Notification.Create(userId, type, title, body, payloadJson);
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
        return notification.Id;
    }

    public async Task<IReadOnlyList<Domain.Notification>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly) query = query.Where(n => n.ReadAt == null);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken ct = default)
            => _db.Notifications.CountAsync(n => n.UserId == userId && n.ReadAt == null, ct);

    public async Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification is null) return;
        notification.MarkRead();
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null)
            .ToListAsync(ct);

        foreach (var n in unread) n.MarkRead();
        await _db.SaveChangesAsync(ct);
    }
}