namespace ChatApp.API.Modules.Main.Application.DTOs;

public record ChatRoomDto(
    Guid Id,
    string Name,
    bool IsGroup,
    DateTimeOffset? LastMessageAt,
    IReadOnlyList<Guid> MemberIds);
