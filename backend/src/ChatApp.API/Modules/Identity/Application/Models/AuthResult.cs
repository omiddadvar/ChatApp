namespace ChatApp.API.Modules.Identity.Application.Models;

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId,
    string UserName,
    string? DisplayName);