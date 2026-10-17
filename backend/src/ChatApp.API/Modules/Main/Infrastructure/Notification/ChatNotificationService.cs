using ChatApp.API.Hubs;
using ChatApp.API.Modules.Main.Application.Abstractions;
using ChatApp.API.Modules.Main.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Modules.Main.Infrastructure.Notification;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public ChatNotificationService(IHubContext<ChatHub, IChatClient> hub)
    {
        _hub = hub;
    }

    public Task NotifyNewMessageAsync(Guid chatRoomId, MessageEnvelope message, CancellationToken ct = default)
        => _hub.Clients.Group(RoomGroup(chatRoomId)).ReceiveMessage(message);

    public Task NotifyRoomMembershipChangedAsync(Guid chatRoomId, Guid userId, bool joined, CancellationToken ct = default)
        => _hub.Clients.Group(RoomGroup(chatRoomId)).RoomMembershipChanged(chatRoomId, userId, joined);

    public Task NotifyContactAddedAsync(Guid ownerUserId, Guid contactUserId, CancellationToken ct = default)
        => _hub.Clients.User(ownerUserId.ToString())
            .Notification("ContactAdded", contactUserId.ToString());

    private static string RoomGroup(Guid roomId) => $"room:{roomId}";
}