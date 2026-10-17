using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Constants;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.API.Modules.Identity.Infrastructure.Seeders;

public class IdentitySeeder : ISeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHostEnvironment _env;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<AppUser> userManager,
        IHostEnvironment env,
        ILogger<IdentitySeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _env = env;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in AppRoles.All)
        {
            if (await _roleManager.RoleExistsAsync(role))
                continue;

            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (result.Succeeded)
            {
                _logger.LogInformation("Seeded role {Role}", role);
            }
            else
            {
                _logger.LogError("Failed to seed role {Role}: {Errors}",
                    role, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        if (!_env.IsDevelopment())
            return;

        const string adminEmail = "admin@chatapp.local";
        const string adminPassword = "Admin@12345";

        var existing = await _userManager.FindByEmailAsync(adminEmail);
        if (existing is not null)
            return;

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Administrator",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to seed admin: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(admin, AppRoles.Admin);
        _logger.LogInformation("Seeded admin user {Email}", adminEmail);
    }
}