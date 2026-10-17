using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Notification.Infrastructure.Persistence;
using ChatApp.API.Modules.Notification.Infrastructure.Services;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Notification;

public static class ServiceConfiguration
{
    public static IServiceCollection AddNotificationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("NotificationDb"));

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationDispatcher, SignalRNotificationDispatcher>();

        return services;
    }

}
