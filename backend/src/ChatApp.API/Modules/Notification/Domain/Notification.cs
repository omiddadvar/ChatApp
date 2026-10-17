using ChatApp.API.Modules.Notification.Domain.Enums;
using ChatApp.API.Modules.Shared.Kernel.Entities;
using ChatApp.API.Modules.Shared.Kernel.Exceptions;

namespace ChatApp.API.Modules.Notification.Domain;

public class Notification : AggregateRoot
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? PayloadJson { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }

    public bool IsRead => ReadAt is not null;
    public bool IsDelivered => DeliveredAt is not null;

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? payloadJson = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(
                "Notification title is required.");

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Body = body?.Trim() ?? string.Empty,
            PayloadJson = payloadJson
        };
    }

    public void MarkDelivered() => DeliveredAt = DateTimeOffset.UtcNow;
    public void MarkRead() => ReadAt = DateTimeOffset.UtcNow;
}