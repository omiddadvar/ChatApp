using ChatApp.API.Modules.Main.Infrastructure.Mongo;
using ChatApp.API.Modules.Shared.Infrastructure.Mongo;

namespace ChatApp.API.Modules.Main;

public static class WebAppConfiguration
{
    public static async Task MainModuleWebAppConfig(this WebApplication app)
    {

        using (var scope = app.Services.CreateScope())
        {
            var mongoContext = scope.ServiceProvider.GetRequiredService<IMongoContext>();
            await ChatHistoryRepository.EnsureIndexesAsync(mongoContext);
        }
    }
}
