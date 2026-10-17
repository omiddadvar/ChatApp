using ChatApp.API.Modules.Main.Application.Interfaces;
using ChatApp.API.Modules.Main.Infrastructure.Mongo;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.Interceptors;
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

        return services;
    }
}