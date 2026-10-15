namespace ChatApp.API.Modules.Shared.Kernel.Constants;

public static class AppRoles
{
    public const string Admin = nameof(Admin);
    public const string User = nameof(User);

    public static readonly string[] All = { Admin, User };
}