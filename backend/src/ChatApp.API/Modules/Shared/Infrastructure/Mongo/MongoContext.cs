using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChatApp.API.Modules.Shared.Infrastructure.Mongo;

public sealed class MongoContext : IMongoContext
{
    private readonly IMongoClient _client;

    public IMongoDatabase Database { get; }

    public MongoContext(IMongoClient client, IOptions<MongoSettings> settings)
    {
        _client = client;
        Database = _client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
        => Database.GetCollection<T>(name);
}