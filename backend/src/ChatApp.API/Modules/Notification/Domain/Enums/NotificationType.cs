using System.ComponentModel;

namespace ChatApp.API.Modules.Notification.Domain.Enums;

public enum NotificationType : byte
{
    [Description("Unknown")]
    Unknown = 0,
    [Description("NewMessage")]
    NewMessage = 1,
    [Description("ContactAdded")]
    ContactAdded = 2,
    [Description("RoomInvite")]
    RoomInvite = 3,
    [Description("System")]
    System = 4,
}
