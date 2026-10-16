
using ChatApp.API.Modules.Identity.Domain;

namespace ChatApp.API.Modules.Identity.Application.Abstractions;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(AppUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user, string? ipAddress);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken token, string? ipAddress, string? replacedBy = null);
    Task RevokeAllUserTokensAsync(Guid userId, string? ipAddress);
}