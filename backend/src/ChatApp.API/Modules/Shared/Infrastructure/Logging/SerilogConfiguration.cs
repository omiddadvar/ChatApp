using Serilog;

namespace ChatApp.API.Modules.Shared.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static IHostBuilder AddChatAppSerilog(
        this IHostBuilder host,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ChatApp.Api")
            .CreateLogger();

        return host.UseSerilog();
    }
}