using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using System.Reflection;

namespace ChatApp.API.Modules.Shared.Kernel.Mediator;

public static class MediatorExtensions
{
    /// <summary>
    /// Registers the Sender as ISender/IMediator and scans the given assemblies
    /// for IRequestHandler & INotificationHandler implementations.
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<IPublisher, Sender>();
        services.AddScoped<IMediator, Sender>();

        if (assemblies is null || assemblies.Length == 0)
            assemblies = new[] { Assembly.GetCallingAssembly() };

        RegisterHandlers(services, assemblies);
        RegisterNotificationHandlers(services, assemblies);
        RegisterPipelineBehaviors(services, assemblies);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterface = typeof(IRequestHandler<,>);
        var types = assemblies
            .SelectMany(a => SafeGetTypes(a))
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => new { Implementation = t, Interface = i }));

        foreach (var t in types)
            services.AddScoped(t.Interface, t.Implementation);
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterface = typeof(INotificationHandler<>);
        var types = assemblies
            .SelectMany(a => SafeGetTypes(a))
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => new { Implementation = t, Interface = i }));

        foreach (var t in types)
            services.AddScoped(t.Interface, t.Implementation);
    }

    private static void RegisterPipelineBehaviors(IServiceCollection services, Assembly[] assemblies)
    {
        var behaviorInterface = typeof(IPipelineBehavior<,>);
        var types = assemblies
            .SelectMany(a => SafeGetTypes(a))
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == behaviorInterface)
                .Select(i => new { Implementation = t, Interface = i }));

        foreach (var t in types)
            services.AddScoped(t.Interface, t.Implementation);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
    }
}