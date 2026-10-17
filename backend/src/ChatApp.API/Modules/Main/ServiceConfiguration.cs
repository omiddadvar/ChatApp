using ChatApp.API.Modules.Main.Application.Abstractions;
using ChatApp.API.Modules.Main.Application.Interfaces;
using ChatApp.API.Modules.Main.Infrastructure.Mongo;
using ChatApp.API.Modules.Main.Infrastructure.Notification;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Main.Infrastructure.Seeders;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.Interceptors;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main;

public static class ServiceConfiguration
{
    public static IServiceCollection AddMainModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MainDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServer"));

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
        services.AddScoped<IChatNotificationService, ChatNotificationService>();

        services.AddKeyedScoped<ISeeder, MainSeeder>("Main");

        return services;
    }
}