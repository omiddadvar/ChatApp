using ChatApp.API.Modules.Main.Application.Interfaces;
using ChatApp.API.Modules.Main.Domain.Documents;
using ChatApp.API.Modules.Shared.Infrastructure.Mongo;
using MongoDB.Driver;

namespace ChatApp.API.Modules.Main.Infrastructure.Mongo;

public class ChatHistoryRepository : IChatHistoryRepository
{
    private const string CollectionName = "chat_messages";
    private readonly IMongoCollection<ChatMessageDocument> _collection;

    public ChatHistoryRepository(IMongoContext mongoContext)
    {
        _collection = mongoContext.GetCollection<ChatMessageDocument>(CollectionName);
    }

    public Task InsertAsync(ChatMessageDocument message, CancellationToken ct = default)
        => _collection.InsertOneAsync(message, cancellationToken: ct);

    public async Task<IReadOnlyList<ChatMessageDocument>> GetHistoryAsync(
        Guid chatRoomId, int skip, int take, CancellationToken ct = default)
    {
        var filter = Builders<ChatMessageDocument>.Filter
            .Eq(x => x.ChatRoomId, chatRoomId);

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.SentAt)
            .Skip(skip)
            .Limit(take)
            .ToListAsync(ct);
    }

    public Task<long> CountAsync(Guid chatRoomId, CancellationToken ct = default)
    {
        var filter = Builders<ChatMessageDocument>.Filter
            .Eq(x => x.ChatRoomId, chatRoomId);
        return _collection.CountDocumentsAsync(filter, cancellationToken: ct);
    }

    public static async Task EnsureIndexesAsync(IMongoContext mongoContext)
    {
        var collection = mongoContext.GetCollection<ChatMessageDocument>(CollectionName);

        var index = new CreateIndexModel<ChatMessageDocument>(
            Builders<ChatMessageDocument>.IndexKeys
                .Ascending(x => x.ChatRoomId)
                .Descending(x => x.SentAt),
            new CreateIndexOptions { Background = true, Name = "ix_room_sentAt" });

        await collection.Indexes.CreateOneAsync(index);
    }
}