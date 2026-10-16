using ChatApp.API.Modules.Shared.Kernel.Mediator;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace ChatApp.API.Modules.Shared.Kernel;

public static class ServiceConfiguration
{
    public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatorConfiguration(assemblies);

        return services;
    }
    private static void AddMediatorConfiguration(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        services.TryAddScoped<Sender>();
        services.TryAddScoped<ISender>(sp => sp.GetRequiredService<Sender>());
        services.TryAddScoped<IPublisher>(sp => sp.GetRequiredService<Sender>());
        services.TryAddScoped<IMediator>(sp => sp.GetRequiredService<Sender>());

        services.RegisterRequestHandlers(assemblies);
        services.RegisterNotificationHandlers(assemblies);
        services.RegisterPipelineBehaviors(assemblies);
    }
    private static void RegisterRequestHandlers(
            this IServiceCollection services,
            Assembly[] assemblies)
    {
        var openInterface = typeof(IRequestHandler<,>);

        foreach (var type in GetConcreteTypes(assemblies))
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == openInterface);

            foreach (var @interface in interfaces)
            {
                services.TryAddScoped(@interface, type);
            }
        }
    }
    private static void RegisterNotificationHandlers(
            this IServiceCollection services,
            Assembly[] assemblies)
    {
        var openInterface = typeof(INotificationHandler<>);

        foreach (var type in GetConcreteTypes(assemblies))
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == openInterface);

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, type);
            }
        }
    }
    private static void RegisterPipelineBehaviors(
            this IServiceCollection services,
            Assembly[] assemblies)
    {
        var openInterface = typeof(IPipelineBehavior<,>);

        foreach (var type in GetConcreteTypes(assemblies))
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == openInterface);

            foreach (var @interface in interfaces)
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Scoped(@interface, type));
            }
        }
    }
    private static IEnumerable<Type> GetConcreteTypes(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies.Distinct())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Partial load — take whatever we could load
                types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                if (type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
                {
                    yield return type;
                }
            }
        }
    }
}
