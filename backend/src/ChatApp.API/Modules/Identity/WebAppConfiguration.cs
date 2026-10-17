using ChatApp.API.Modules.Shared.Kernel.Abstractions;

namespace ChatApp.API.Modules.Identity;

public static class WebAppConfiguration
{
    public static async Task IdentityModuleWebAppConfig(this WebApplication app)
    {

        using (var scope = app.Services.CreateScope())
        {
            var mainSeeder = scope.ServiceProvider.GetRequiredKeyedService<ISeeder>("Identity");
            await mainSeeder.SeedAsync();
        }
    }
}