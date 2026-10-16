using ChatApp.API.Modules.Shared.Infrastructure;
using ChatApp.API.Modules.Shared.Kernel;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddSharedKernel();

builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly,
    includeInternalTypes: true);


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
