using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Main.Domain;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Infrastructure.Seeders;

public class MainSeeder : ISeeder
{
    private readonly MainDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHostEnvironment _env;
    private readonly ILogger<MainSeeder> _logger;

    public MainSeeder(
        MainDbContext db,
        UserManager<AppUser> userManager,
        IHostEnvironment env,
        ILogger<MainSeeder> logger)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (!_env.IsDevelopment())
            return;

        var admin = await _userManager.FindByEmailAsync("admin@chatapp.local");
        if (admin is null)
        {
            _logger.LogWarning("MainSeeder skipped — admin user not found.");
            return;
        }

        var alice = await EnsureUserAsync("alice@chatapp.local", "Alice", "Alice@12345");
        var bob = await EnsureUserAsync("bob@chatapp.local", "Bob", "Bob@12345");

        if (alice is null || bob is null)
            return;

        await EnsureContactAsync(admin.Id, alice.Id, "Alice (Admin's Contact)", ct);
        await EnsureContactAsync(admin.Id, bob.Id, null, ct);

        await EnsureDirectChatAsync(admin.Id, alice.Id, ct);

        await EnsureGroupChatAsync(admin.Id, "Demo Group", new[] { alice.Id, bob.Id }, ct);

        await _db.SaveChangesAsync(ct);
    }

    private async Task<AppUser?> EnsureUserAsync(string email, string displayName, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null)
            return user;

        user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to seed user {Email}: {Errors}",
                email, string.Join("; ", result.Errors.Select(e => e.Description)));
            return null;
        }

        await _userManager.AddToRoleAsync(
            user, ChatApp.API.Modules.Shared.Kernel.Constants.AppRoles.User);

        _logger.LogInformation("Seeded demo user {Email}", email);
        return user;
    }

    private async Task EnsureContactAsync(
        Guid ownerId, Guid contactId, string? alias, CancellationToken ct)
    {
        var exists = await _db.Contacts
            .AnyAsync(c => c.OwnerUserId == ownerId && c.ContactUserId == contactId, ct);

        if (exists) return;

        _db.Contacts.Add(Contact.Create(ownerId, contactId, alias));
        _logger.LogInformation("Seeded contact {OwnerId} → {ContactId}", ownerId, contactId);
    }

    private async Task EnsureDirectChatAsync(
        Guid userA, Guid userB, CancellationToken ct)
    {
        var exists = await _db.ChatRooms
            .AnyAsync(r => !r.IsGroup
                           && r.Members.Any(m => m.UserId == userA)
                           && r.Members.Any(m => m.UserId == userB), ct);

        if (exists) return;

        _db.ChatRooms.Add(ChatRoom.CreateDirect(userA, userB));
        _logger.LogInformation("Seeded direct chat between {A} and {B}", userA, userB);
    }

    private async Task EnsureGroupChatAsync(
        Guid creatorId, string name, Guid[] memberIds, CancellationToken ct)
    {
        var exists = await _db.ChatRooms
            .AnyAsync(r => r.IsGroup && r.Name == name, ct);

        if (exists) return;

        _db.ChatRooms.Add(ChatRoom.CreateGroup(creatorId, name, memberIds));
        _logger.LogInformation("Seeded group chat {Name}", name);
    }
}