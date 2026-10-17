using ChatApp.API.Modules.Main.Application.Abstractions;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly MainDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ChatHub(MainDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }


    public override async Task OnConnectedAsync()
    {
        if (_currentUser.UserId is null)
        {
            Context.Abort();
            return;
        }

        var roomIds = await _db.ChatRooms
            .Where(r => r.Members.Any(m => m.UserId == _currentUser.UserId))
            .Select(r => r.Id)
            .ToListAsync();

        foreach (var roomId in roomIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(Guid chatRoomId)
    {
        if (_currentUser.UserId is null)
            throw new HubException("Unauthenticated.");

        var isMember = await _db.ChatRooms
            .AnyAsync(r => r.Id == chatRoomId
                           && r.Members.Any(m => m.UserId == _currentUser.UserId));

        if (!isMember)
            throw new HubException("You are not a member of this room.");

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(chatRoomId));
    }

    public Task LeaveRoom(Guid chatRoomId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(chatRoomId));

    public async Task StartTyping(Guid chatRoomId)
    {
        if (_currentUser.UserId is null) return;

        var isMember = await _db.ChatRooms
            .AnyAsync(r => r.Id == chatRoomId
                           && r.Members.Any(m => m.UserId == _currentUser.UserId));

        if (!isMember) return;

        await Clients.OthersInGroup(RoomGroup(chatRoomId))
            .UserTyping(chatRoomId, _currentUser.UserId.Value, _currentUser.UserName ?? "User");
    }

    public Task StopTyping(Guid chatRoomId)
    {
        if (_currentUser.UserId is null)
            return Task.CompletedTask;

        return Clients.OthersInGroup(RoomGroup(chatRoomId))
            .UserStoppedTyping(chatRoomId, _currentUser.UserId.Value);
    }

    private static string RoomGroup(Guid roomId) => $"room:{roomId}";
}