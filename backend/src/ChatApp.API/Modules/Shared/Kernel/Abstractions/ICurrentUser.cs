namespace ChatApp.API.Modules.Shared.Kernel.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}