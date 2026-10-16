namespace ChatApp.API.Modules.Identity.Application.Models;

public record UserProfileDto(
    Guid Id,
    string Email,
    string UserName,
    string? DisplayName,
    string? AvatarUrl,
    IEnumerable<string> Roles,
    DateTimeOffset CreatedAt);