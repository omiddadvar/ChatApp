namespace ChatApp.API.Modules.Shared.Infrastructure.Mongo;

public sealed class MongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "Chat_DB";
}