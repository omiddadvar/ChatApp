using ChatApp.API.Modules.Identity.Application.Abstractions;
using ChatApp.API.Modules.Identity.Application.Services;
using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Identity.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.Interceptors;
using ChatApp.API.Modules.Shared.Kernel.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ChatApp.API.Modules.Identity;

public static class ServiceConfiguration
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDBContext(configuration);

        services.AddIdentityConfigs();

        services.AddAuthenticationConfigs(configuration);

        services.AddAuthorizationConfigs();

        services.AddScoped<ITokenService, TokenService>();

        return services;
    }


    private static void AddDBContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("IdentityDb"),
                sql =>
                {
                    sql.MigrationsHistoryTable("__IdentityMigrationsHistory");
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });
    }

    private static void AddIdentityConfigs(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 1;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager();
    }

    private static void AddAuthenticationConfigs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(signingKey)),

                ClockSkew = TimeSpan.FromMinutes(1),

                NameClaimType = AppClaims.UserName,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken)
                        && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                        .CreateLogger("JwtBearer");

                    logger.LogWarning(context.Exception,
                        "JWT authentication failed for {Path}",
                        context.HttpContext.Request.Path);

                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    context.HandleResponse();

                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        return context.Response.WriteAsync(
                            "{\"error\":\"unauthorized\",\"message\":\"Authentication is required.\"}");
                    }

                    return Task.CompletedTask;
                }
            };
        });
    }

    private static void AddAuthorizationConfigs(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole(AppRoles.Admin));
    }
}