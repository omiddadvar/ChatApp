using ChatApp.API.Modules.Shared.Kernel.Entities;

namespace ChatApp.API.Modules.Main.Domain;

public class ChatRoomMember : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public bool IsAdmin { get; set; }
    public DateTimeOffset? LeftAt { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;
}