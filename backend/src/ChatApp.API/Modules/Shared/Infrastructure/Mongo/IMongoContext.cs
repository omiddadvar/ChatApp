using MongoDB.Driver;

namespace ChatApp.API.Modules.Shared.Infrastructure.Mongo;

public interface IMongoContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<T> GetCollection<T>(string name);
}
