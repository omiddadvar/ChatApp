using ChatApp.API.Modules.Main.Application.DTOs;

namespace ChatApp.API.Modules.Main.Application.Abstractions;

public interface IChatNotificationService
{
    Task NotifyNewMessageAsync(Guid chatRoomId, MessageEnvelope message, CancellationToken ct = default);
    Task NotifyRoomMembershipChangedAsync(Guid chatRoomId, Guid userId, bool joined, CancellationToken ct = default);
    Task NotifyContactAddedAsync(Guid ownerUserId, Guid contactUserId, CancellationToken ct = default);
}