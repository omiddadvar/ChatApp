namespace ChatApp.API.Modules.Shared.Kernel.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}