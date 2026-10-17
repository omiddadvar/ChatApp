using ChatApp.API.Modules.Shared.Kernel.Entities;
using ChatApp.API.Modules.Shared.Kernel.Exceptions;

namespace ChatApp.API.Modules.Main.Domain;

public class ChatRoom : AggregateRoot
{
    private readonly List<ChatRoomMember> _members = new();

    public string Name { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public bool IsGroup { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }

    public IReadOnlyList<ChatRoomMember> Members => _members.AsReadOnly();

    private ChatRoom() { }

    public static ChatRoom CreateDirect(Guid creatorUserId, Guid otherUserId)
    {
        if (creatorUserId == otherUserId)
            throw new DomainException("Cannot create a direct chat with yourself.");

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = creatorUserId,
            IsGroup = false,
            Name = string.Empty
        };

        room._members.Add(new ChatRoomMember
        {
            ChatRoomId = room.Id,
            UserId = creatorUserId,
            JoinedAt = DateTimeOffset.UtcNow
        });

        room._members.Add(new ChatRoomMember
        {
            ChatRoomId = room.Id,
            UserId = otherUserId,
            JoinedAt = DateTimeOffset.UtcNow
        });

        return room;
    }

    public static ChatRoom CreateGroup(Guid creatorUserId, string name, IEnumerable<Guid> memberIds)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Group name is required.");

        if (name.Length > 100)
            throw new DomainException("Group name must not exceed 100 characters.");

        var distinctMembers = memberIds
            .Where(id => id != creatorUserId)
            .Distinct()
            .ToList();

        if (distinctMembers.Count == 0)
            throw new DomainException("A group must have at least one other member.");

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = creatorUserId,
            IsGroup = true,
            Name = name.Trim()
        };

        room._members.Add(new ChatRoomMember
        {
            ChatRoomId = room.Id,
            UserId = creatorUserId,
            JoinedAt = DateTimeOffset.UtcNow,
            IsAdmin = true
        });

        foreach (var memberId in distinctMembers)
        {
            room._members.Add(new ChatRoomMember
            {
                ChatRoomId = room.Id,
                UserId = memberId,
                JoinedAt = DateTimeOffset.UtcNow
            });
        }

        return room;
    }

    public void MarkMessageActivity(DateTimeOffset when)
    {
        LastMessageAt = when;
    }

    public bool HasMember(Guid userId) => _members.Any(m => m.UserId == userId);
}