using ChatApp.API.Modules.Main.Application.DTOs;

namespace ChatApp.API.Modules.Main.Application.Abstractions;


public interface IChatClient
{
    Task ReceiveMessage(MessageEnvelope message);

    Task UserTyping(Guid chatRoomId, Guid userId, string userName);

    Task UserStoppedTyping(Guid chatRoomId, Guid userId);

    Task RoomMembershipChanged(Guid chatRoomId, Guid userId, bool joined);

    Task MessageRead(Guid chatRoomId, Guid messageId, Guid userId);

    Task Notification(string type, string payload);
}
