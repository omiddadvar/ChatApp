using ChatApp.API.Hubs;
using ChatApp.API.Modules.Identity;
using ChatApp.API.Modules.Main;
using ChatApp.API.Modules.Notification;
using ChatApp.API.Modules.Shared.Infrastructure;
using ChatApp.API.Modules.Shared.Kernel;
using FluentValidation;

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


var app = builder.Build();

await app.MainModuleWebAppConfig();
await app.IdentityModuleWebAppConfig();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");


app.Run();
