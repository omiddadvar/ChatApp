using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Notification.Application.DTOs;
using ChatApp.API.Modules.Notification.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Hubs;


[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    private readonly NotificationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationHub(
        NotificationDbContext db,
        ICurrentUser currentUser,
        IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _currentUser = currentUser;
        _scopeFactory = scopeFactory;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUser.UserId is null)
        {
            Context.Abort();
            return;
        }

        var userId = _currentUser.UserId.Value;

        await DeliverPendingAsync(userId);

        await base.OnConnectedAsync();
    }

    private async Task DeliverPendingAsync(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var pending = await db.Notifications
            .Where(n => n.UserId == userId && n.DeliveredAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        if (pending.Count == 0) return;

        foreach (var notification in pending)
        {
            await Clients.Caller.ReceiveNotification(new NotificationEnvelope(
                notification.Id,
                (byte)notification.Type,
                notification.Title,
                notification.Body,
                notification.PayloadJson,
                notification.CreatedAt));

            notification.MarkDelivered();
        }

        await db.SaveChangesAsync();
    }
}