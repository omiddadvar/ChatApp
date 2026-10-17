using ChatApp.API.Modules.Notification.Domain.Enums;

namespace ChatApp.API.Modules.Notification.Application.DTOs;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Body,
    string? PayloadJson,
    bool IsRead,
    DateTimeOffset CreatedAt);
