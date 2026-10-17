namespace ChatApp.API.Modules.Notification.Application.DTOs;

public record NotificationEnvelope(
    Guid Id,
    byte Type,
    string Title,
    string Body,
    string? PayloadJson,
    DateTimeOffset CreatedAt);