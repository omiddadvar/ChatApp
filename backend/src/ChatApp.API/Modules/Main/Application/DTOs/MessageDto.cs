using ChatApp.API.Modules.Main.Domain.Enums;

namespace ChatApp.API.Modules.Main.Application.DTOs;

public record MessageDto(
    Guid Id,
    Guid SenderUserId,
    string Content,
    Guid? AttachmentId,
    MessageType MessageType,
    DateTimeOffset SentAt);