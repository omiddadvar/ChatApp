using static ChatApp.API.Modules.Main.Application.Queries.GetChatHistory;

namespace ChatApp.API.Modules.Main.Application.DTOs;

public record ChatHistoryDto(
    Guid ChatRoomId,
    IReadOnlyList<MessageDto> Messages,
    long TotalCount,
    int Page,
    int PageSize);
