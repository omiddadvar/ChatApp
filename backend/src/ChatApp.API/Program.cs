using ChatApp.API.Hubs;
using ChatApp.API.Modules.Identity;
using ChatApp.API.Modules.Identity.Infrastructure.Persistence;
using ChatApp.API.Modules.Main;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Notification;
using ChatApp.API.Modules.Notification.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Infrastructure;
using ChatApp.API.Modules.Shared.Kernel;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddSharedKernel();

builder.Services.AddIdentityModule(builder.Configuration);

builder.Services.AddMainModule(builder.Configuration);

builder.Services.AddNotificationModule(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    includeInternalTypes: true);


// CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ChatAppCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

await app.MainModuleWebAppConfig();
await app.IdentityModuleWebAppConfig();

// Configure the HTTP request pipeline.
app.UseCors("ChatAppCors");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");


if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        await Task.WhenAll
        (
            services.GetRequiredService<IdentityDbContext>().Database.MigrateAsync(),
            services.GetRequiredService<MainDbContext>().Database.MigrateAsync(),
            services.GetRequiredService<NotificationDbContext>().Database.MigrateAsync()
        );
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed");
        throw;
    }
}

app.Run();
