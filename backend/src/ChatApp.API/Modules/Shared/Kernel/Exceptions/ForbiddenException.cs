namespace ChatApp.API.Modules.Shared.Kernel.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You are not allowed to perform this action.")
        : base(message) { }
}