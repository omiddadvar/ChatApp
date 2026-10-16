using ChatApp.API.Modules.Shared.Kernel.Abstractions;

namespace ChatApp.API.Modules.Shared.Infrastructure.Security;

public interface ICurrentUserService : ICurrentUser
{
    IEnumerable<string> Roles { get; }
    string? GetClaim(string type);
}
