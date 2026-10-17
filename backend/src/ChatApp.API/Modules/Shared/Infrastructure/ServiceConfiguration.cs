using ChatApp.API.Modules.Shared.Infrastructure.Mongo;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.Interceptors;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.Repositories;
using ChatApp.API.Modules.Shared.Infrastructure.Security;
using ChatApp.API.Modules.Shared.Infrastructure.Storage;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minio;
using MongoDB.Driver;

namespace ChatApp.API.Modules.Shared.Infrastructure;

public static class ServiceConfiguration
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddCurrentUserInfrastructure();

        services.AddEfInterceptorInfrastructure();

        services.AddMinioInfrastructure(configuration);

        services.AddMongoInfrastructure(configuration);

        services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();


        return services;
    }
    private static void AddMongoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoSettings>(
            configuration.GetSection(MongoSettings.SectionName));

        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = configuration
                .GetSection(MongoSettings.SectionName)
                .Get<MongoSettings>() ?? new MongoSettings();

            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoContext, MongoContext>();
    }
    private static void AddMinioInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MinioSettings>(
    configuration.GetSection(MinioSettings.SectionName));

        services.AddSingleton<IMinioClient>(_ =>
        {
            var settings = configuration
                .GetSection(MinioSettings.SectionName)
                .Get<MinioSettings>() ?? new MinioSettings();

            var builder = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (settings.UseSSL)
                builder = builder.WithSSL();

            return builder.Build();
        });

        services.AddSingleton<IFileStorage, MinioFileStorage>();
    }
    private static void AddEfInterceptorInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
    }
    private static void AddCurrentUserInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserService>());
    }
}
