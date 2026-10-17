using System.ComponentModel;

namespace ChatApp.API.Modules.Main.Domain.Enums;

public enum MessageType : byte
{
    [Description("Unknown")]
    Unknown = 0,
    [Description("Text")]
    Text = 1,
    [Description("Image")]
    Image = 2,
    [Description("File")]
    File = 3,
    [Description("System")]
    System = 4,
}
