using ChatApp.API.Modules.Main.Domain.Enums;

namespace ChatApp.API.Modules.Main.Application.DTOs;

public record MessageEnvelope(
    Guid Id,
    Guid ChatRoomId,
    Guid SenderUserId,
    string SenderUserName,
    string Content,
    Guid? AttachmentId,
    MessageType MessageType,
    DateTimeOffset SentAt);