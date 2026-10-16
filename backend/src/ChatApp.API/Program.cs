using ChatApp.API.Modules.Shared.Infrastructure;
using ChatApp.API.Modules.Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddSharedKernel();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
