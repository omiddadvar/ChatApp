using ChatApp.API.Modules.Main.Domain.Documents;

namespace ChatApp.API.Modules.Main.Application.Interfaces;

public interface IChatHistoryRepository
{
    Task InsertAsync(ChatMessageDocument message, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDocument>> GetHistoryAsync(
        Guid chatRoomId, int skip, int take, CancellationToken ct = default);
    Task<long> CountAsync(Guid chatRoomId, CancellationToken ct = default);
}