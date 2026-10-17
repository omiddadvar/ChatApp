using ChatApp.API.Modules.Main.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatApp.API.Modules.Main.Domain.Documents;

public class ChatMessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid ChatRoomId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid SenderUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public Guid? AttachmentId { get; set; }

    public DateTimeOffset SentAt { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Text;

    public bool IsDeleted { get; set; }
}