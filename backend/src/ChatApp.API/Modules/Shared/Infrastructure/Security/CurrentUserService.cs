using ChatApp.API.Modules.Shared.Kernel.Constants;
using System.Security.Claims;

namespace ChatApp.API.Modules.Shared.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var id = Principal?.FindFirst(AppClaims.UserId)?.Value
                     ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? UserName =>
        Principal?.FindFirst(AppClaims.UserName)?.Value
        ?? Principal?.FindFirst(ClaimTypes.Name)?.Value
        ?? Principal?.Identity?.Name;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public string? GetClaim(string type) => Principal?.FindFirst(type)?.Value;
}