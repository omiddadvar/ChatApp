using ChatApp.API.Modules.Shared.Kernel.Constants;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Modules.Shared.Infrastructure.Security;

public class SignalRUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(AppClaims.UserId)?.Value
           ?? connection.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}