using ChatApp.API.Hubs;
using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Notification.Application.DTOs;
using ChatApp.API.Modules.Notification.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Notification.Infrastructure.Services;


public class SignalRNotificationDispatcher : INotificationDispatcher
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hub;
    private readonly NotificationDbContext _db;

    public SignalRNotificationDispatcher(
        IHubContext<NotificationHub, INotificationClient> hub,
        NotificationDbContext db)
    {
        _hub = hub;
        _db = db;
    }

    public async Task<bool> DispatchAsync(
        Domain.Notification notification,
        CancellationToken ct = default)
    {
        await _hub.Clients.User(notification.UserId.ToString())
            .ReceiveNotification(new NotificationEnvelope(
                notification.Id,
                (byte)notification.Type,
                notification.Title,
                notification.Body,
                notification.PayloadJson,
                notification.CreatedAt));

        notification.MarkDelivered();
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task DispatchPendingAsync(
        Guid userId,
        string connectionId,
        CancellationToken ct = default)
    {
        var pending = await _db.Notifications
            .Where(n => n.UserId == userId && n.DeliveredAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var notification in pending)
        {
            await _hub.Clients.Client(connectionId).ReceiveNotification(new NotificationEnvelope(
                notification.Id,
                (byte)notification.Type,
                notification.Title,
                notification.Body,
                notification.PayloadJson,
                notification.CreatedAt));

            notification.MarkDelivered();
        }

        await _db.SaveChangesAsync(ct);
    }
}